using System.Collections.Generic;

public static class LobbyData
{
    public static bool isOnlineGame = false;

    public static List<PlayerSelection> players = new List<PlayerSelection>();
}

public struct PlayerSelection
{
    public int selectedCharacter;
    public string playerName;
}