using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyCoreMvc.Initialization;
using MyCoreMvc.Repositorys;
using WebMVC.Filter;

namespace WebMVC
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

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

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
            services.AddDistributedMemoryCache();
            services.AddSession();

            #region ��һ��Ȩ�޹���
            services.AddMvc(options =>
            {
                options.Filters.Add<Filter.CheckLoginAuthorizeFilter>();
                options.Filters.Add<Filter.MyExceptionFilter>();
            });
            #endregion




            #region �ڶ���Ȩ�޹���
            //Ȩ��Ҫ�����
            var permissionRequirement = new PermissionRequirement(
                "/Home/visitDeny",// �ܾ���Ȩ����ת��ַ
                ClaimTypes.Name,//�����û�������Ȩ
                expiration: TimeSpan.FromSeconds(60 * 5)//�ӿڵĹ���ʱ��
                );

            //����Ȩ��
            services.AddAuthorization(options =>
            {
                options.AddPolicy("Permission", policy => policy.Requirements.Add(permissionRequirement));
            });
            // ע��Ȩ�޴�����
            services.AddTransient<IAuthorizationHandler, PermissionHandler>();
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
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSession();
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
