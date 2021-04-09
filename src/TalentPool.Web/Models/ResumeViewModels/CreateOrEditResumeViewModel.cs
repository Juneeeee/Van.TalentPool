using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TalentPool.Application.Resumes;
using TalentPool.Resumes;

namespace TalentPool.Web.Models.ResumeViewModels
{
    public class CreateOrEditResumeViewModel
    {
        public Guid? Id { get; set; }

   
        public List<SelectListItem> Jobs { get; set; }
        [Required(ErrorMessage = "请选择职位")]
        public Guid JobId { get; set; }

        public List<SelectListItem> Platforms { get; set; }
        [Required(ErrorMessage = "请选择招聘平台")]
        public string PlatformName { get; set; }
        [Required(ErrorMessage = "请输入招聘平台ID")]
        public string PlatformId { get; set; }

        public bool ActiveDelivery { get; set; }

        /* Edit ↓ */
        public string Name { get; set; }

        public string City { get; set; }

        [RegularExpression(@"1[\d]{10}",ErrorMessage ="请输入正确的手机号码")]
        public string PhoneNumber { get; set; }
        [RegularExpression(@"[\d]*", ErrorMessage = "请输入正确的分机号码")]
        public string ExtensionNumber { get; set; }


        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        public string Description { get; set; }
          
        public string Keywords { get; set; }
        public bool IgnoreSimilarity { get; set; }
        public List<ResumeCompareDto> ResumeCompares { get; set; }

        public bool Enable { get; set; } = true;
        public AuditStatus AuditStatus { get; set; }
        public Guid OwnerUserId { get; set; }
    }
}
