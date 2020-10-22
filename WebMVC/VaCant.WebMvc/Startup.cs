using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Impl;
using VaCant.EFCore;
using VaCant.Initialization;
using VaCant.WebMvc.Filter;

namespace VaCant.WebMvc
{
    public class Startup
    {
        //log4net��־
        //public static ILoggerRepository repository { get; set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            //log4net
            // repository = LogManager.CreateRepository("NETCoreRepository");
            //ָ�������ļ�
            // XmlConfigurator.Configure(repository, new FileInfo("log4net.config"));
        }

        public IConfiguration Configuration { get; }
        //ConfigureServices��ӷ���͹��ܣ�  Configure������Ӻõķ����ʹ�÷�ʽ
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //AddRazorRuntimeCompilation ����ʹҳ������ǰһ�������޸�ҳ��ʵʱˢ�£�������Ҫ��������
            services.AddControllersWithViews().AddRazorRuntimeCompilation(); ;
            #region ��ע��
            //ע������������ݿ�
            //services.AddDbContext<SqlServerDbContext>(options =>
            //{
            //    options.UseSqlServer(Configuration.GetConnectionString("Default"));//��ȡ���õ������ַ���
            //});
            //�ص�ִ��ͷ���ע�뷽ʽҪһ��
            //����ע��ִ�  
            //services.AddScoped(typeof(IRepository<,>), typeof(MyRepository<,>));
            //services.AddScoped(typeof(IRepository<>), typeof(MyRepository<>));

            //services.AddScoped<IUserService,UserService>();
            //services.AddScoped<IRoleService, RoleService>();
            #endregion

            //���ù���ģʽ��������ע��
            InitializationFactory.Injection(services, Configuration);
            //�����ڴ�洢Session
            //services.AddDistributedMemoryCache();
            services.AddSession();

            #region ��һ��Ȩ�޹���
            //ȫ��ע��Filter
            services.AddMvc(options =>
            {
                // options.Filters.Add(typeof(Filter.VaCantAuthorizationFilter));
                // options.Filters.Add<Filter.CheckLoginAuthorizeFilter>();
                options.Filters.Add<Filter.MyExceptionFilter>();
               // options.Filters.Add<Filter.MyActionFilter>();
            });
            //ֻ�ڿ�������action��[ServiceFilter(typeof(VaCantAuthorizationFilter))]������
            services.AddScoped<VaCantAuthorizationFilter>();
            //services.AddAuthentication(option =>
            //{
            //    option.DefaultScheme = "Cookie";
            //    option.DefaultChallengeScheme = "Cookie";
            //    option.DefaultAuthenticateScheme = "Cookie";
            //    option.DefaultForbidScheme = "Cookie";
            //    option.DefaultSignInScheme = "Cookie";
            //    option.DefaultSignOutScheme = "Cookie";
            //}).AddCookie("Cookie", option =>
            //{
            //    option.LoginPath = "/Account/Login";
            //    option.AccessDeniedPath = "/Account/Forbidden";
            //    //.......
            //});
            #endregion

            #region Quartz��ʱ����

            //ע��ISchedulerFactory��ʵ��
            //services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
            //StartQuartzAsync();
            #endregion

            #region �ڶ���Ȩ�޹���
            ////Ȩ��Ҫ�����
            //var permissionRequirement = new PermissionRequirement(
            //    "/Home/visitDeny",// �ܾ���Ȩ����ת��ַ
            //    ClaimTypes.Name,//�����û�������Ȩ
            //    expiration: TimeSpan.FromSeconds(60 * 5)//�ӿڵĹ���ʱ��
            //    );

            ////����Ȩ��
            //services.AddAuthorization(options =>
            //{
            //    options.AddPolicy("Permission", policy => policy.Requirements.Add(permissionRequirement));
            //});
            //// ע��Ȩ�޴�����
            //services.AddTransient<IAuthorizationHandler, PermissionHandler>();
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSession();
            app.UseRouting();
            //���������֤
            app.UseAuthorization();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                     //pattern: "{controller=Home}/{action=Index}/{id?}");
                     pattern: "{controller=Account}/{action=Login}/{id?}");
            });
        }

        /// <summary>
        /// Quartz�첽ִ�ж�ʱ����
        /// </summary>
        /// <returns></returns>
        private async Task StartQuartzAsync()
        {
            // �ο����� https://www.cnblogs.com/MicroHeart/p/9402731.html
            //https://www.cnblogs.com/dangzhensheng/p/10496278.html
            StdSchedulerFactory _schedulerFactory = new StdSchedulerFactory();
            //1.ͨ���������õ�����
            IScheduler _scheduler = await _schedulerFactory.GetScheduler();
            //2.����������
            await _scheduler.Start();
            //3.����������(Ҳ��ʱ�����)
            var trigger = TriggerBuilder.Create()
                            .WithSimpleSchedule(x => x.WithIntervalInSeconds(3).RepeatForever())//ÿ10��ִ��һ��
                            .Build();
            //4.������ҵʵ��
            //Jobs��������Ҫִ�е���ҵ
            var jobDetail = JobBuilder.Create<DemoJob>()
                            .WithIdentity("Myjob", "group")//���Ǹ������ҵȡ�˸���Myjob�������֣���ȡ�˸�����Ϊ��group��
                            .Build();
            //5.������������ҵ����󶨵���������
            await _scheduler.ScheduleJob(jobDetail, trigger);
        }
    }
}
