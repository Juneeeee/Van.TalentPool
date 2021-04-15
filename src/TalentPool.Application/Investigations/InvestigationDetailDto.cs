using System;
using TalentPool.Investigations;

namespace TalentPool.Application.Investigations
{
    public class InvestigationDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid ResumeId { get; set; }
        public string PhoneNumber { get; set; }
        public string ExtensionNumber { get; set; }
        public string Email { get; set; }
        public Guid JobId { get; set; }
        public string JobName { get; set; }
        public bool ActiveDelivery { get; set; }
        public DateTime InvestigateDate { get; set; }
        public bool? IsQualified { get; set; }
        public string QualifiedRemark { get; set; }
        public bool? IsConnected { get; set; }
        public string UnconnectedRemark { get; set; }
        public InvestigationStatus Status { get; set; }
        public AcceptTravelStatus? AcceptTravelStatus { get; set; }
        public string NotAcceptTravelReason { get; set; }
        public string ExpectedSalary { get; set; }
        public WorkState? WorkState { get; set; }
        public bool? IsAcceptInterview { get; set; }
        public string ExpectedDate { get; set; }
        public string ExpectedInterviewDate { get; set; }
        public string ExpectedPhoneInterviewDate { get; set; }
        public string Information { get; set; }
        public string Evaluation { get; set; }
        public string CityOfResidence { get; set; }
        public string CityOfDomicile { get; set; }
        public string CreatorUserName { get; set; }
        public DateTime CreationTime { get; set; }
        public string LastModifierUserName { get; set; }
        public DateTime? LastModificationTime { get; set; }
        public string OwnerUserName { get; set; } 
    }
}
