using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance;

    [SerializeField] private TMP_InputField loginInput;
    [SerializeField] private TMP_InputField passwordInput;

    [SerializeField] private TMP_InputField registerLoginInput;
    [SerializeField] private TMP_InputField registerPasswordInput;
    [SerializeField] private TMP_InputField registerNameInput;

    [SerializeField] private TMP_Text messageText;
    [SerializeField] private MainMenuManager menuManager;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button registerButton;

    private bool isRequestInProgress;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void OnLoginClicked()
    {
        if (isRequestInProgress)
            return;

        string login = loginInput.text.Trim();
        string password = passwordInput.text;

        if (login == "")
        {
            SetMessage("Введите логин");
            return;
        }

        if (password == "")
        {
            SetMessage("Введите пароль");
            return;
        }

        StartCoroutine(LoginCoroutine(login, password));
    }

    public void OnRegisterClicked()
    {
        if (isRequestInProgress)
            return;

        string login = registerLoginInput.text.Trim();
        string password = registerPasswordInput.text;
        string name = registerNameInput.text.Trim();

        if (login == "")
        {
            SetMessage("Введите логин");
            return;
        }

        if (password == "")
        {
            SetMessage("Введите пароль");
            return;
        }

        if (name == "")
        {
            SetMessage("Введите имя");
            return;
        }

        StartCoroutine(RegisterCoroutine(login, password, name));
    }

    private IEnumerator LoginCoroutine(string login, string password)
    {
        SetBusy(true);

        LoginRequest request = new LoginRequest
        {
            login = login,
            password = password
        };

        yield return ApiManager.Instance.PostRequest(
            ApiRoutes.Login,
            JsonUtility.ToJson(request),
            OnLoginSuccess,
            OnRequestError
        );

        SetBusy(false);
    }

    private IEnumerator RegisterCoroutine(string login, string password, string name)
    {
        SetBusy(true);

        RegisterRequest request = new RegisterRequest
        {
            login = login,
            password = password,
            name = name
        };

        yield return ApiManager.Instance.PostRequest(
            ApiRoutes.Register,
            JsonUtility.ToJson(request),
            OnRegisterSuccess,
            OnRequestError
        );

        SetBusy(false);
    }

    private void OnLoginSuccess(string responseJson)
    {
        HandleAuthSuccess(responseJson, "Неверный логин или пароль");
    }

    private void OnRegisterSuccess(string responseJson)
    {
        HandleAuthSuccess(responseJson, "Не удалось зарегистрироваться");
    }

    private void HandleAuthSuccess(string responseJson, string fallbackError)
    {
        AuthResponse response = JsonUtility.FromJson<AuthResponse>(responseJson);

        if (response != null &&
            !string.IsNullOrEmpty(response.token) &&
            response.user != null)
        {
            PlayerSession.SaveSession(
                response.user.id,
                response.user.login,
                response.user.name,
                response.token
            );

            SetMessage("");
            if (menuManager != null)
            {
                menuManager.OnAuthenticated();
            }
        }
        else
        {
            SetMessage(fallbackError);
        }
    }

    private void OnRequestError(string error)
    {
        Debug.LogError(error);
        ErrorResponse response = JsonUtility.FromJson<ErrorResponse>(error);
        SetMessage(response != null && !string.IsNullOrEmpty(response.error)
            ? response.error
            : "Ошибка запроса к серверу");
    }

    private void SetBusy(bool busy)
    {
        isRequestInProgress = busy;

        if (loginButton != null)
        {
            loginButton.interactable = !busy;
        }

        if (registerButton != null)
        {
            registerButton.interactable = !busy;
        }

        SetInputEnabled(loginInput, !busy);
        SetInputEnabled(passwordInput, !busy);
        SetInputEnabled(registerLoginInput, !busy);
        SetInputEnabled(registerPasswordInput, !busy);
        SetInputEnabled(registerNameInput, !busy);
    }

    private void SetInputEnabled(TMP_InputField input, bool enabled)
    {
        if (input != null)
        {
            input.interactable = enabled;
        }
    }

    private void SetMessage(string text)
    {
        if (messageText != null)
        {
            messageText.text = text;
        }
    }
}
