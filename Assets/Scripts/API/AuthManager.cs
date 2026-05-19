using System.Collections;
using TMPro;
using UnityEngine;

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

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void OnLoginClicked()
    {
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
    }

    private IEnumerator RegisterCoroutine(string login, string password, string name)
    {
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
    }

    private void OnLoginSuccess(string responseJson)
    {
        AuthResponse response = JsonUtility.FromJson<AuthResponse>(responseJson);

        if (response != null && response.status && response.user != null)
        {
            PlayerSession.SaveSession(
                response.user.id,
                response.user.login,
                response.user.name,
                response.user.totalScore,
                response.token
            );

            SetMessage("");
            menuManager.ShowMainMenu();
        }
        else
        {
            SetMessage("Неверный логин или пароль");
        }
    }

    private void OnRegisterSuccess(string responseJson)
    {
        SetMessage("Регистрация успешна. Теперь войди.");
        menuManager.ShowLogin();
    }

    private void OnRequestError(string error)
    {
        Debug.LogError(error);
        SetMessage("Ошибка запроса к серверу");
    }

    private void SetMessage(string text)
    {
        messageText.text = text;
    }
}