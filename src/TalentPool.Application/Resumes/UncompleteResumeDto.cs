using System;
using TalentPool.Investigations;

namespace TalentPool.Application.Resumes
{
    public class UncompleteResumeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid JobId { get; set; }
        public string JobName { get; set; }
        public virtual bool? IsConnected { get; set; }
        public virtual AcceptTravelStatus? AcceptTravelStatus { get; set; }
        public virtual string ExpectedInterviewDate { get; set; }
        public DateTime CreationTime { get; set; }
        public Guid OwnerUserId { get; set; }
        public string OwnerUserName { get; set; }
        public Guid? InvestigationId { get; set; }
        public InvestigationStatus? Status { get; set; }
        public DateTime? InvestigationDate { get; set; }
        public DateTime? AppointmentTime { get; set; }
        public AppointmentType? AppointmentType { get; set; }
    }
}
