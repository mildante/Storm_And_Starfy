using System;

[Serializable]
public class RegisterRequest
{
    public string login;
    public string password;
    public string name;
}

[Serializable]
public class LoginRequest
{
    public string login;
    public string password;
}

[Serializable]
public class UserDto
{
    public int id;
    public string login;
    public string name;
    public int totalScore;
}

[Serializable]
public class AuthResponse
{
    public bool status;
    public string token;
    public UserDto user;
    public string error;
}

[Serializable]
public class UserProgressItem
{
    public int id;
    public int userId;
    public int levelNumber;
    public bool isUnlocked;
    public bool isCompleted;
}

[Serializable]
public class ProgressResponse
{
    public bool status;
    public UserProgressItem[] progress;
}

[Serializable]
public class CompleteLevelRequest
{
    public int userId;
    public int levelNumber;
    public int score;
}

[Serializable]
public class CompleteLevelResponse
{
    public bool status;
    public int totalScore;
}

[Serializable]
public class LeaderboardUser
{
    public string name;
    public int totalScore;
}

[Serializable]
public class LeaderboardResponse
{
    public bool status;
    public LeaderboardUser[] leaderboard;
}