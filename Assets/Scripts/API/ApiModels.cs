using System;

[Serializable]
public class RegisterRequest
{
    public string login;
    public string name;
    public string password;
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
}

[Serializable]
public class AuthResponse
{
    public string token;
    public UserDto user;
}

[Serializable]
public class MeResponse
{
    public UserDto user;
}

[Serializable]
public class ErrorResponse
{
    public string error;
}
