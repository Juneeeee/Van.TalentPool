using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TalentPool.Application;
using TalentPool.Application.Resumes;
using TalentPool.Investigations;
using TalentPool.Resumes;

namespace TalentPool.EntityFrameworkCore.Queriers
{
    public class ResumeQuerier : IResumeQuerier
    {
        private readonly TalentDbContext _context;
        private ISignal _signal;
        public ResumeQuerier(TalentDbContext context, ISignal signal)
        {
            _context = context;
            _signal = signal;
        }
        protected CancellationToken CancellationToken => _signal.Token;

        public async Task<PaginationOutput<ResumeDto>> GetListAsync(QueryResumeInput input)
        {
            CancellationToken.ThrowIfCancellationRequested();
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            var query = from a in _context.Resumes
                        join b in _context.Investigations on a.Id equals b.ResumeId into bb
                        from bbb in bb.DefaultIfEmpty()
                        join c in _context.Jobs on a.JobId equals c.Id
                        join d in _context.Users on a.CreatorUserId equals d.Id
                        join e in _context.Users on a.OwnerUserId equals e.Id
                        select new ResumeDto
                        {
                            Id = a.Id,
                            Name = a.Name,
                            JobId = a.JobId,
                            JobName = c.Title,
                            PhoneNumber = a.PhoneNumber,
                            CreationTime = a.CreationTime,
                            CreatorUserId = a.CreatorUserId,
                            CreatorUserName = d.FullName,
                            OwnerUserId = a.OwnerUserId,
                            OwnerUserName = e.FullName,
                            InvestigationId = bbb.Id,
                            AuditStatus = a.AuditStatus,
                            PlatformName = a.PlatformName,
                            PlatformId = a.PlatformId
                        };

            if (!string.IsNullOrEmpty(input.Keyword))
            {
                query = query.Where(w => w.Name.Contains(input.Keyword) || w.PhoneNumber.Contains(input.Keyword) || w.PlatformId.Contains(input.Keyword));
                if (Guid.TryParse(input.Keyword, out var id))
                {
                    query = query.Where(w => w.Id == id);
                }
            }

            if (input.JobId.HasValue)
                query = query.Where(w => w.JobId == input.JobId.Value);
            if (input.CreatorUserId.HasValue)
                query = query.Where(w => w.CreatorUserId == input.CreatorUserId.Value);
            if (input.OwnerUserId.HasValue && input.OwnerUserId != Guid.Empty)
                query = query.Where(w => w.OwnerUserId == input.OwnerUserId.Value);
            if (input.StartTime.HasValue && input.EndTime.HasValue)
                query = query.Where(w => w.CreationTime >= input.StartTime.Value && w.CreationTime <= input.EndTime.Value);
            if (input.AuditStatus.HasValue)
                query = query.Where(w => w.AuditStatus == (AuditStatus)input.AuditStatus.Value);

            var totalCount = await query.CountAsync(CancellationToken);
            var totalSize = (int)Math.Ceiling(totalCount / (decimal)input.PageSize);
            var resumes = await query.OrderByDescending(o => o.CreationTime)
                 .Skip((input.PageIndex - 1) * input.PageSize)
                .Take(input.PageSize)
                 .ToListAsync(CancellationToken);
            return new PaginationOutput<ResumeDto>(totalSize, resumes);
        }


        public async Task<ResumeDetailDto> GetResumeAsync(Guid id)
        {
            CancellationToken.ThrowIfCancellationRequested();
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            var query = from a in _context.Resumes
                        join b in _context.Investigations on a.Id equals b.ResumeId into bb
                        from bbb in bb.DefaultIfEmpty()
                        join c in _context.Jobs on a.JobId equals c.Id
                        join d in _context.Users on a.CreatorUserId equals d.Id
                        join e in _context.Users on a.OwnerUserId equals e.Id
                        join f in _context.Users on a.LastModifierUserId equals f.Id into ff
                        from fff in ff.DefaultIfEmpty()
                        where a.Id == id
                        select new ResumeDetailDto
                        {
                            Id = a.Id,
                            Name = a.Name,
                            JobId = a.JobId,
                            JobName = c.Title,
                            PhoneNumber = a.PhoneNumber,
                            ExtensionNumber = a.ExtensionNumber,
                            CreationTime = a.CreationTime,
                            CreatorUserId = a.CreatorUserId,
                            CreatorUserName = d.FullName,
                            OwnerUserId = a.OwnerUserId,
                            OwnerUserName = e.FullName,
                            InvestigationId = bbb == null ? (Guid?)null : bbb.Id,
                            AuditStatus = a.AuditStatus,
                            PlatformName = a.PlatformName,
                            PlatformId = a.PlatformId,
                            City = a.City,
                            Email = a.Email,
                            Description = a.Description,
                            LastModificationTime = a.LastModificationTime,
                            LastModifierUserName = fff == null ? string.Empty : fff.FullName,
                            ActiveDelivery = a.ActiveDelivery
                        };
            var resume = await query.FirstOrDefaultAsync(CancellationToken);
            if (resume != null)
            {
                resume.ResumeAuditRecords = await GetResumeAuditRecordsAsync(id);
                resume.ResumeCompares = await _context.ResumeCompares
                    .Where(w => w.ResumeId == id)
                    .Select(s => new ResumeCompareDto()
                    {
                        RelationResumeId = s.RelationResumeId,
                        RelationResumeName = s.RelationResumeName,
                        Similarity = s.Similarity
                    }).ToListAsync(CancellationToken);

                resume.Attachments = await _context.Attachments
                  .Where(w => w.ResumeId == id)
                  .Select(s => new ResumeAttachmentDto()
                  {
                      FileName = s.FileName,
                      FilePath = s.FilePath,
                      CreationTime = s.CreationTime
                  }).ToListAsync(CancellationToken);

                resume.Keywords = await _context.ResumeKeyMaps
                    .Where(w => w.ResumeId == id)
                    .Select(s => s.Keyword).ToListAsync();
            }
            return resume;

        }

        public async Task<List<ResumeAuditRecordDto>> GetResumeAuditRecordsAsync(Guid resumeId)
        {
            CancellationToken.ThrowIfCancellationRequested();
            if (resumeId == null)
                throw new ArgumentNullException(nameof(resumeId));
            var query = from a in _context.ResumeAuditRecords
                        join b in _context.Users on a.CreatorUserId equals b.Id
                        where a.ResumeId == resumeId
                        select new ResumeAuditRecordDto()
                        {

                            Id = a.Id,
                            CreatorUserId = a.CreatorUserId,
                            CreationTime = a.CreationTime,
                            CreatorUserName = b.FullName,
                            Passed = a.Passed,
                            Remark = a.Remark
                        };

            return await query.ToListAsync(CancellationToken);
        }

        public async Task<List<StatisticResumeDto>> GetStatisticResumesAsync(DateTime startTime, DateTime endTime, AuditStatus? auditStatus)
        {
            CancellationToken.ThrowIfCancellationRequested();
            if (startTime == null)
                throw new ArgumentNullException(nameof(startTime));
            if (endTime == null)
                throw new ArgumentNullException(nameof(endTime));
            var query = from a in _context.Resumes
                        join b in _context.Users on a.CreatorUserId equals b.Id
                        join c in _context.Jobs on a.JobId equals c.Id
                        where a.CreationTime >= startTime && a.CreationTime <= endTime
                        select new StatisticResumeDto()
                        {
                            OwnerUserId = a.OwnerUserId,
                            CreatorUserId = a.CreatorUserId,
                            CreationTime = a.CreationTime,
                            CreatorUserName = b.FullName,
                            AuditStatus = a.AuditStatus,
                            CreatorUserPhoto = b.Photo,
                            JobName = c.Title,
                            ActiveDelivery = a.ActiveDelivery
                        };
            if (auditStatus.HasValue)
                query = query.Where(w => w.AuditStatus == auditStatus);

            return await query.ToListAsync(CancellationToken);
        }

        public async Task<List<UncompleteResumeDto>> GetUncompleteResumesAsync(Guid? ownerUserId)
        {
            CancellationToken.ThrowIfCancellationRequested();

            var query = from a in _context.Resumes
                        join b in _context.Investigations on a.Id equals b.ResumeId into bb
                        from bbb in bb.DefaultIfEmpty()
                        join c in _context.Jobs on a.JobId equals c.Id
                        join d in _context.Users on a.OwnerUserId equals d.Id
                        where a.AuditStatus == AuditStatus.Complete && (bbb == null || bbb.Status != InvestigationStatus.Complete)
                        orderby a.CreationTime
                        select new UncompleteResumeDto
                        {
                            Id = a.Id,
                            Name = a.Name,
                            JobId = a.JobId,
                            JobName = c.Title,
                            CreationTime = a.CreationTime,
                            OwnerUserId = a.OwnerUserId,
                            OwnerUserName = d.FullName,
                            InvestigationId = bbb == null ? (Guid?)null : bbb.Id,
                            Status = bbb == null ? (InvestigationStatus?)null : bbb.Status,
                            InvestigationDate = bbb == null ? (DateTime?)null : bbb.InvestigateDate,
                            AppointmentTime = bbb == null ? null : bbb.AppointmentTime,
                            AppointmentType = bbb == null ? null : bbb.AppointmentType,
                            IsConnected = bbb == null ? null : bbb.IsConnected,
                            AcceptTravelStatus = bbb == null ? null : bbb.AcceptTravelStatus,
                            ExpectedInterviewDate = bbb == null ? null : bbb.ExpectedInterviewDate,
                        };
            if (ownerUserId.HasValue)
                query = query.Where(w => w.OwnerUserId == ownerUserId);
            return await query.ToListAsync(CancellationToken);
        }


        public async Task<List<ResumeExportDto>> GetExportResumesAsync(QueryExportResumeInput input)
        {
            CancellationToken.ThrowIfCancellationRequested();
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            var query = from a in _context.Resumes
                        join c in _context.Jobs on a.JobId equals c.Id
                        join d in _context.Users on a.CreatorUserId equals d.Id
                        join e in _context.Users on a.OwnerUserId equals e.Id
                        select new ResumeExportDto
                        {
                            Id = a.Id,
                            Name = a.Name,
                            JobId = a.JobId,
                            JobName = c.Title,
                            PhoneNumber = a.PhoneNumber,
                            CreationTime = a.CreationTime,
                            AuditStatus = a.AuditStatus,
                            PlatformName = a.PlatformName,
                            PlatformId = a.PlatformId,
                            CreatorUserId = a.CreatorUserId,
                            OwnerUserId = a.OwnerUserId,
                            City = a.City,
                            Description = a.Description
                        };

            if (!string.IsNullOrEmpty(input.Keyword))
                query = query.Where(w => w.Name.Contains(input.Keyword)
                || w.PhoneNumber.Contains(input.Keyword)
               || w.PlatformId.Contains(input.Keyword));
            if (input.JobId.HasValue)
                query = query.Where(w => w.JobId == input.JobId.Value);
            if (input.CreatorUserId.HasValue)
                query = query.Where(w => w.CreatorUserId == input.CreatorUserId.Value);
            if (input.OwnerUserId.HasValue)
                query = query.Where(w => w.OwnerUserId == input.OwnerUserId.Value);
            if (input.StartTime.HasValue && input.EndTime.HasValue)
                query = query.Where(w => w.CreationTime >= input.StartTime.Value && w.CreationTime <= input.EndTime.Value);
            if (input.AuditStatus.HasValue)
                query = query.Where(w => w.AuditStatus == (AuditStatus)input.AuditStatus.Value);

            return await query.ToListAsync(CancellationToken);
        }
    }
}
