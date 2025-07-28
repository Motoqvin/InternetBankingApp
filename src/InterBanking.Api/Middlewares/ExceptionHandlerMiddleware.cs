using InterBanking.Api.Exceptions;
using InterBanking.Api.Responses;

namespace InterBanking.Api.Middlewares;
public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate next;
    public ExceptionHandlerMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext) {
        try
        {
            await this.next.Invoke(httpContext);
        }
        catch (ArgumentNullException ex)
        {
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(new BadRequestResponse(message: ex.Message)
            {
                Parameter = ex.ParamName
            });

            httpContext.Items["exception"] = ex.ToString();
        }
        catch (NotFoundException ex)
        {
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            await httpContext.Response.WriteAsJsonAsync(new NotFoundResponse(message: ex.Message)
            {
                Parameter = ex.ParamName
            });

            httpContext.Items["exception"] = ex.ToString();
        }
        catch (BadRequestException ex)
        {
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(new BadRequestResponse(message: ex.Message)
            {
                Parameter = ex.ParamName
            });

            httpContext.Items["exception"] = ex.ToString();
        }
        catch (UserLockedException ex)
        {
            httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
            await httpContext.Response.WriteAsJsonAsync(new BadRequestResponse(message: ex.Message));

            httpContext.Items["exception"] = ex.ToString();
        }
        catch (WrongCredentialsException ex)
        {
            httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
            await httpContext.Response.WriteAsJsonAsync(new BadRequestResponse(message: ex.Message));

            httpContext.Items["exception"] = ex.ToString();
        }
        catch (UnauthorizedAccessException ex)
        {
            httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await httpContext.Response.WriteAsJsonAsync(new BadRequestResponse(message: ex.Message));

            httpContext.Items["exception"] = ex.ToString();
        }
        catch (Exception ex)
        {
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await httpContext.Response.WriteAsJsonAsync(new InternalServerErrorResponse(message: "Internal server error"));

            httpContext.Items["exception"] = ex.ToString();
        }
    }
}