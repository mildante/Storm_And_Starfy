using UnityEngine;

public static class PlayerSession
{
    public static int UserId;
    public static string Login;
    public static string Name;
    public static string Token;

    public static bool IsAuthorized => UserId > 0 && !string.IsNullOrEmpty(Token);

    public static void SaveSession(int userId, string login, string name, string token)
    {
        UserId = userId;
        Login = login;
        Name = name;
        Token = token;

        PlayerPrefs.SetInt("user_id", userId);
        PlayerPrefs.SetString("user_login", login);
        PlayerPrefs.SetString("user_name", name);
        PlayerPrefs.SetString("auth_token", token);
        PlayerPrefs.Save();
    }

    public static void UpdateUser(UserDto user)
    {
        if (user == null)
            return;

        UserId = user.id;
        Login = user.login;
        Name = user.name;

        PlayerPrefs.SetInt("user_id", UserId);
        PlayerPrefs.SetString("user_login", Login);
        PlayerPrefs.SetString("user_name", Name);
        PlayerPrefs.Save();
    }

    public static void LoadSession()
    {
        UserId = PlayerPrefs.GetInt("user_id", 0);
        Login = PlayerPrefs.GetString("user_login", "");
        Name = PlayerPrefs.GetString("user_name", "");
        Token = PlayerPrefs.GetString("auth_token", "");
    }

    public static void SaveToken(string token)
    {
        Token = token;
        PlayerPrefs.SetString("auth_token", token);
        PlayerPrefs.Save();
    }

    public static void Clear()
    {
        UserId = 0;
        Login = "";
        Name = "";
        Token = "";

        PlayerPrefs.DeleteKey("user_id");
        PlayerPrefs.DeleteKey("user_login");
        PlayerPrefs.DeleteKey("user_name");
        PlayerPrefs.DeleteKey("user_total_score");
        PlayerPrefs.DeleteKey("auth_token");
    }
}
