using UnityEngine;

public static class ApiRoutes
{
    public const string BaseUrl = "https://localhost:7112/api";

    public const string Register = BaseUrl + "/User/registrationUser";
    public const string Login = BaseUrl + "/User/authUser";
    public const string GetUser = BaseUrl + "/User/getUser";

    public const string GetProgress = BaseUrl + "/Game/getProgress";
    public const string CompleteLevel = BaseUrl + "/Game/completeLevel";
    public const string GetLeaderboard = BaseUrl + "/Game/getLeaderboard";
}
