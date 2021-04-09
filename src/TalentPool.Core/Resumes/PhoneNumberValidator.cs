using System;
using System.Threading.Tasks;

namespace TalentPool.Resumes
{
    public class PhoneNumberValidator : IResumeValidator
    {
        public async Task ValidateAsync(ResumeManager manager, Resume resume)
        {
            if (resume == null)
                throw new ArgumentNullException(nameof(resume));
            if (string.IsNullOrEmpty(resume.PhoneNumber))
                return;
            var owner = await manager.FindByPhoneNumberAsync(resume.PhoneNumber, resume.ExtensionNumber);
            if (owner != null && owner.Id != resume.Id)
            {
                if (string.IsNullOrEmpty(resume.ExtensionNumber))
                    throw new InvalidOperationException($"{resume.PhoneNumber}的简历已存在，简历ID：{owner.Id}。");
                else
                    throw new InvalidOperationException($"{resume.PhoneNumber} 分机号{resume.ExtensionNumber} 的简历已存在，简历ID：{owner.Id}。");
            }
        }
    }
}
