using Microsoft.AspNetCore.Mvc;
using Quartz;
using System.Threading.Tasks;

public class JobController : Controller
{
    private readonly IScheduler _scheduler;

    public JobController(IScheduler scheduler)
    {
        _scheduler = scheduler;
    }

    public async Task<IActionResult> TriggerEmailJob()
    {
        var jobKey = new JobKey("EmailJob", "Group1");
        var job = await _scheduler.GetJobDetail(jobKey);

        if (job != null)
        {
            await _scheduler.TriggerJob(jobKey);
            return Content("Email job triggered.");
        }

        return Content("Job not found.");
    }
}
