using Quartz;
using Quartz.Impl;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using InboxWarmUp; 
using Microsoft.EntityFrameworkCore;
using InboxWarmUp.Models;
using InboxWarmUp.Services;
using Microsoft.Extensions.Configuration;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews(); 
builder.Services.AddSession();

builder.Services.AddDbContext<EmailDbContext>(options =>
       options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


// Add EmailService to DI
builder.Services.AddTransient<EmailService>();
builder.Services.AddTransient<EmailReplyService>();
builder.Services.AddHttpClient<AIService>();  
builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory(); 
});


builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
builder.Services.AddTransient<DomainVerificationService>();
var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.UseSession();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");


app.Lifetime.ApplicationStarted.Register(async () =>
{
    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var scheduler = await scope.ServiceProvider.GetRequiredService<ISchedulerFactory>().GetScheduler();
            await scheduler.Start();

            // Schedule the existing EmailJob
            IJobDetail emailJob = JobBuilder.Create<EmailJob>()
                .WithIdentity("EmailJob", "Group1")
                .Build();

            ITrigger emailTrigger = TriggerBuilder.Create()
                .WithIdentity("EmailTrigger", "Group1")
                .WithDailyTimeIntervalSchedule(x => x.WithIntervalInMinutes(1))
                .Build();

            await scheduler.ScheduleJob(emailJob, emailTrigger);

            // Schedule the new ReplyJob to check email replies
            IJobDetail replyJob = JobBuilder.Create<ReplyJob>()
                .WithIdentity("ReplyJob", "Group1")
                .Build();

            ITrigger replyTrigger = TriggerBuilder.Create()
                .WithIdentity("ReplyTrigger", "Group1")
                .WithDailyTimeIntervalSchedule(x => x.WithIntervalInMinutes(60)) // Check every 5 minutes
                .Build();

            await scheduler.ScheduleJob(replyJob, replyTrigger);
            Console.WriteLine("Reply job scheduled successfully.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error occurred: {ex.Message}");
    }
});


app.Run();
