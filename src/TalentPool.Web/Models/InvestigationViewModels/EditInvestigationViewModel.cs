using System;
using System.ComponentModel.DataAnnotations;
using TalentPool.Investigations;

namespace TalentPool.Web.Models.InvestigationViewModels
{
    public class EditInvestigationViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } 
        public string PhoneNumber { get; set; }
        public string ExtensionNumber { get; set; }
        public string Email { get; set; }
        public Guid ResumeId { get; set; }
        public Guid JobId { get; set; }
        public string JobName { get; set; }
        public bool ActiveDelivery { get; set; }
        //调查时间
        public DateTime InvestigateDate { get; set; }

        // 是否成功电话联系
        [Required(ErrorMessage = "请确定电话接通情况")]
        public bool? IsConnected { get; set; }

        public string UnconnectedRemark { get; set; }
        // 是否可出差
        public AcceptTravelStatus? AcceptTravelStatus { get; set; }
        // 不出差的理由
        public string NotAcceptTravelReason { get; set; }
        // 期望薪水
        public string ExpectedSalary { get; set; }
        // 工作状态
        public WorkState? WorkState { get; set; }
        // 是否接受现场面试
        public bool? IsAcceptInterview { get; set; }
        // 预期可上班日期
        public string ExpectedDate { get; set; }
        // 预期可面试日期
        public string ExpectedInterviewDate { get; set; }
        // 预期可电话面试日期
        public string ExpectedPhoneInterviewDate { get; set; }

        // 调查信息
        public string Information { get; set; }
        // 技术评测
        public string Evaluation { get; set; }
        // 预约时间
        public DateTime? AppointmentTime { get; set; }
        /// <summary>
        /// 预约内容
        /// </summary>
        public AppointmentType? AppointmentType { get; set; }

        // 居住地城市
        public string CityOfResidence { get; set; }

        // 户籍地城市 
        public string CityOfDomicile { get; set; }
    }
}
