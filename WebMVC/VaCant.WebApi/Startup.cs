using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System.IO;
using System.Linq;

namespace VaCant.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddMemoryCache();
            //���swagger�ĵ�
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1.0.0",
                    Title = "�ӿ��ĵ�"
                });
                //// ��ȡxml�ļ���
                //var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                //// ��ȡxml�ļ�·��
                //var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                //// ��ӿ�������ע�ͣ�true��ʾ��ʾ������ע��
                //options.IncludeXmlComments(xmlPath, true);

                // JWT��֤
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Type = SecuritySchemeType.Http,
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Description = "Authorization:Bearer {your JWT token}<br/><b>��Ȩ��ַ:/Base_Manage/Home/SubmitLogin</b>",
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] { }
                    }
                });
                // Ϊ Swagger JSON and UI����xml�ĵ�ע��·��
                //��ȡӦ�ó�������Ŀ¼�����ԣ����ܹ���Ŀ¼Ӱ�죬������ô˷�����ȡ·����
                var basePath = Path.GetDirectoryName(typeof(Program).Assembly.Location);

                var xmls = Directory.GetFiles(basePath, "*.xml");
                xmls.ToList().ForEach(aXml =>
                {
                    options.IncludeXmlComments(aXml);
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}
            app.UseMiddleware<CorsMiddleware>()//����
            .UseDeveloperExceptionPage()
            .UseStaticFiles(new StaticFileOptions
            {
                ServeUnknownFileTypes = true,
                DefaultContentType = "application/octet-stream"
            });

            // ���Swagger�й��м��
            app.UseSwagger()
            .UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "1.0.0");
                c.RoutePrefix = string.Empty;
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            ApiLog();
            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}

            //app.UseRouting();

            //app.UseAuthorization();

            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapControllers();
            //});
        }

        private void ApiLog()
        {
            //HttpHelper.HandleLog = log =>
            //{
            //    //�ӿ���־
            //    using (var lifescope = AutofacHelper.Container.BeginLifetimeScope())
            //    {
            //        lifescope.Resolve<IMyLogger>().Info(LogType.ϵͳ����, log);
            //    }
            //};
        }
    }
}