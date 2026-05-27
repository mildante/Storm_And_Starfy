public static class ApiRoutes
{
    public static string BaseUrl { get; set; } = "http://127.0.0.1:5051/api";

    public static string Register => BaseUrl + "/auth/register";
    public static string Login => BaseUrl + "/auth/login";
    public static string Me => BaseUrl + "/auth/me";
}
