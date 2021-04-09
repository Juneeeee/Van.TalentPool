using System;
using System.Collections.Generic;
using TalentPool.Entities;

namespace TalentPool.Resumes
{
    /// <summary>
    /// 简历
    /// </summary>
    public class Resume : Entity, ICreationAudited, IModificationAudited, IDeletionAudited
    {
        /// <summary>
        /// 姓名
        /// </summary>
        public virtual string Name { get; set; }
        /// <summary>
        /// 职位
        /// </summary> 
        public virtual Guid JobId { get; set; }
        /// <summary>
        /// 求职投递城市
        /// </summary>
        public virtual string City { get; set; }
        /// <summary>
        /// 电话号码
        /// </summary>
        public virtual string PhoneNumber { get; set; }
        /// <summary>
        /// 分机号
        /// </summary>
        public virtual string ExtensionNumber { get; set; }
        /// <summary>
        /// 邮箱
        /// </summary>
        public virtual string Email { get; set; }
        /// <summary>
        /// 简历描述
        /// </summary>
        public virtual string Description { get; set; }
        /// <summary>
        /// 来源平台
        /// </summary>        
        public virtual string PlatformName { get; set; }
        /// <summary>
        /// 来源平台ID
        /// </summary>
        public virtual string PlatformId { get; set; }
        /// <summary>
        /// 审核状态
        /// </summary>
        public virtual AuditStatus AuditStatus { get; set; }
       
        /// <summary>
        /// 主动投递
        /// </summary>
        public virtual bool ActiveDelivery { get; set; }
        /// <summary>
        /// 简历关键词
        /// </summary>
        public virtual ICollection<ResumeKeywordMap> KeyMaps { get; set; }
        /// <summary>
        /// 相似简历
        /// </summary>
        public virtual ICollection<ResumeCompare>  ResumeCompares { get; set; }
        /// <summary>
        /// 审核记录
        /// </summary>
        public virtual ICollection<ResumeAuditRecord> AuditRecords { get; set; }
        /// <summary>
        /// 附件
        /// </summary>
        public virtual ICollection<ResumeAttachment>  Attachments { get; set; }
      

        /// <summary>
        /// 处理人
        /// </summary>
        public virtual Guid OwnerUserId { get; set; }

        public virtual Guid CreatorUserId { get; set; }
        public virtual DateTime CreationTime { get; set; }
        public virtual Guid? LastModifierUserId { get; set; }
        public virtual DateTime? LastModificationTime { get; set; }
        public virtual Guid? DeleterUserId { get; set; }
        public virtual DateTime? DeletionTime { get; set; }
        public virtual bool IsDeleted { get; set; }
        public virtual string ConcurrencyStamp { get; set; }
    }
}
