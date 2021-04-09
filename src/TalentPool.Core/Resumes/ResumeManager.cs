using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TalentPool.Resumes
{
    public class ResumeManager : ObjectDisposable
    {
        public ResumeManager(IResumeStore resumeStore,
            IResumeAuditSettingStore resumeAuditSettingStore,
            IEnumerable<IResumeValidator> resumeValidators,
            IResumeComparer resumeComparer,
            IOptions<ResumeOptions> options,
            ISignal signal)
        {
            ResumeStore = resumeStore;
            ResumeValidators = resumeValidators;
            ResumeComparer = resumeComparer;
            ResumeAuditSettingStore = resumeAuditSettingStore;
            Options = options?.Value;
            Signal = signal;
        }
        public ResumeOptions Options { get; }
        protected IResumeStore ResumeStore { get; }
        protected ISignal Signal { get; }
        protected IResumeAuditSettingStore ResumeAuditSettingStore { get; }
        protected IEnumerable<IResumeValidator> ResumeValidators { get; }
        protected IResumeComparer ResumeComparer { get; }
        protected virtual CancellationToken CancellationToken => Signal.Token;

        public async Task<Resume> CreateAsync(Resume resume)
        {
            if (resume == null)
                throw new ArgumentNullException(nameof(resume));
            await ValidateAsync(resume);
            return await ResumeStore.CreateAsync(resume, CancellationToken);
        }

        public async Task<Resume> UpdateAsync(Resume resume, bool ignoreDuplicated)
        {
            if (resume == null)
                throw new ArgumentNullException(nameof(resume));
            // 重复性验证
            await ValidateAsync(resume);
            //获取相似简历
            await CompareAsync(resume);
            if (!ignoreDuplicated & resume.ResumeCompares != null && resume.ResumeCompares.Count > 0)
                throw new InvalidOperationException("检测存在相似简历，如忽略请勾选忽略重复选项。");
            return await ResumeStore.UpdateAsync(resume, CancellationToken);
        }

        private async Task ValidateAsync(Resume resume)
        {
            if (ResumeValidators != null)
            {
                foreach (var validator in ResumeValidators)
                {
                    await validator.ValidateAsync(this, resume);
                }
            }
        }
        private async Task CompareAsync(Resume resume)
        {
            if (ResumeComparer != null)
            {
                await ResumeComparer.CompareAsync(this, resume, Options.MinSimilarityValue);
            }
        }
        public async Task DeleteAsync(Resume resume)
        {
            if (resume == null)
                throw new ArgumentNullException(nameof(resume));
            await ResumeStore.DeleteAsync(resume, CancellationToken);
        }
        public async Task<Resume> FindByIdAsync(Guid resumeId)
        {
            if (resumeId == null)
                throw new ArgumentNullException(nameof(resumeId));

            return await ResumeStore.FindByIdAsync(resumeId, CancellationToken);
        }
        public async Task<Resume> FindByPhoneNumberAsync(string phoneNumber, string extensionNumber = "")
        {
            if (phoneNumber == null)
                throw new ArgumentNullException(nameof(phoneNumber));

            return await ResumeStore.FindByPhoneNumberAsync(phoneNumber, extensionNumber, CancellationToken);
        }
        public async Task<Resume> FindByPlatformAsync(string platformId)
        {
            if (platformId == null)
                throw new ArgumentNullException(nameof(platformId));
            return await ResumeStore.FindByPlatformAsync(platformId, CancellationToken);
        }

        public async Task<ResumeAuditRecord> GetAuditRecordByIdAsync(Guid auditRecordId)
        {
            if (auditRecordId == null)
                throw new ArgumentNullException(nameof(auditRecordId));

            return await ResumeStore.GetAuditRecordByIdAsync(auditRecordId, CancellationToken);
        }
        public async Task AuditAsync(Resume resume, bool passed, Guid auditedUserId, ResumeAuditRecord auditRecord)
        {
            if (resume == null)
                throw new ArgumentNullException(nameof(resume));

            if (resume.AuditStatus == AuditStatus.Complete || resume.AuditStatus == AuditStatus.Unpassed)
                throw new InvalidOperationException("当前简历已处理，请勿重复操作。");

            // 检查审批进度
            var auditSettings = await ResumeAuditSettingStore.GetAuditSettingsAsync(CancellationToken);

            // 检查当前用户符合审批条件
            var auditSetting = auditSettings.FirstOrDefault(f => f.UserId == auditedUserId);
            if (auditSetting == null)
                throw new InvalidOperationException("当前用户无审核权限。");
            // 设置审核状态
            // 如果当前节点序号大于或等于整个审批人总数，则表示审批结束
            AuditStatus auditStatus = AuditStatus.Ongoing;
            if (auditSetting.Order >= auditSettings.Count - 1)
                auditStatus = AuditStatus.Complete;
            if (!passed)
                auditStatus = AuditStatus.Unpassed;

            var audit = await ResumeStore.AddAuditRecordAsync(resume, auditRecord, CancellationToken);
            resume.AuditStatus = auditStatus;
            await ResumeStore.UpdateAsync(resume, CancellationToken);
        }
        public async Task CancelAuditAsync(Resume resume, Guid auditedUserId, ResumeAuditRecord auditRecord)
        {

            if (resume == null)
                throw new ArgumentNullException(nameof(resume));
            if (auditedUserId == null)
                throw new ArgumentNullException(nameof(auditedUserId));
            if (auditRecord == null)
                throw new ArgumentNullException(nameof(auditRecord));

            if (auditRecord.CreatorUserId != auditedUserId)
                throw new InvalidOperationException("当前审核记录非当前用户所有，无法撤销操作。");

            await ResumeStore.RemoveAuditRecordAsync(resume, auditRecord, CancellationToken);

            resume.AuditStatus = AuditStatus.Ongoing;
            await ResumeStore.UpdateAsync(resume, CancellationToken);
        }
        public async Task AssignUserAsync(Resume resume, Guid ownerUserId)
        {
            if (resume == null)
                throw new ArgumentNullException(nameof(resume));
            if (ownerUserId == null)
                throw new ArgumentNullException(nameof(ownerUserId));
            resume.OwnerUserId = ownerUserId;
            await ResumeStore.UpdateAsync(resume, CancellationToken);
        }
        internal async Task<List<ResumeKeywordMap>> GetResumeKeyMapsAsync(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
                throw new ArgumentNullException(nameof(keyword));
            return await ResumeStore.GetResumeKeyMapsAsync(keyword, CancellationToken);
        }
        internal async Task<List<ResumeKeywordMap>> GetResumeKeyMapsAsync(Guid resumeId)
        {
            if (resumeId == null)
                throw new ArgumentNullException(nameof(resumeId));
            return await ResumeStore.GetResumeKeyMapsAsync(resumeId, CancellationToken);
        }
        internal async Task RemoveResumeKeyMapsAsync(List<ResumeKeywordMap> resumeKeywordMaps)
        {
            if (resumeKeywordMaps == null)
                throw new ArgumentNullException(nameof(resumeKeywordMaps));
            await ResumeStore.RemoveResumeKeyMapsAsync(resumeKeywordMaps, CancellationToken);
        }

        public async Task AddAttachmentAsync(Resume resume, List<ResumeAttachment> attachments)
        {
            if (resume == null)
                throw new ArgumentNullException(nameof(resume));
            if (attachments == null)
                throw new ArgumentNullException(nameof(attachments));
            if (resume.Attachments == null)
                resume.Attachments = new List<ResumeAttachment>();
            foreach (var attachment in attachments)
            {
                resume.Attachments.Add(attachment);
            }
            await ResumeStore.UpdateAsync(resume, CancellationToken);
        }
        public async Task RemoveAttachmentAsync(Resume resume, ResumeAttachment attachment)
        {
            if (resume == null)
                throw new ArgumentNullException(nameof(resume));
            if (attachment == null)
                throw new ArgumentNullException(nameof(attachment));
            resume.Attachments.Remove(attachment);
            await ResumeStore.UpdateAsync(resume, CancellationToken);
        }

        protected override void DisposeUnmanagedResource()
        {
            ResumeStore.Dispose();
        }
    }
}
