using Scalar.AspNetCore;
using System.Security.Claims;

namespace HR__Management_System.Extentions
{
    public static class ApiMiddlewares
    {
        public static WebApplication UseApiMiddelwares(this WebApplication app)
        {
            app.UseExceptionHandler();

            // ملحوظة: قم بإلغاء UseHttpsRedirection مؤقتاً إذا كنت تستخدم الدومين المؤقت http
            // app.UseHttpsRedirection();

            app.UseCors("AllowAngularApp");

            app.UseRouting();

            // ✅ تفعيل Scalar و OpenAPI على السيرفر في كل البيئات (Production & Development)
            app.MapOpenApi();
            app.MapScalarApiReference(options =>
            {
                options.Title = "HRMS";
                options.Theme = ScalarTheme.Purple;
            });

            app.UseAuthentication();
            app.UseAuthorization();

            return app;
        }
    }
}