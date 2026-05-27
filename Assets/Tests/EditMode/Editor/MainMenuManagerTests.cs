using System.Reflection;
using NUnit.Framework;

public class MainMenuManagerTests
{
    [Test]
    public void CanStartJoinRoomRequest_ReturnsFalse_WhenAlreadyInRoom()
    {
        MethodInfo method = typeof(MainMenuManager).GetMethod(
            "CanStartJoinRoomRequest",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);

        bool canStart = (bool)method.Invoke(
            null,
            new object[]
            {
                false,
                true,
                true,
                true,
                true
            });

        Assert.False(canStart);
    }
}
