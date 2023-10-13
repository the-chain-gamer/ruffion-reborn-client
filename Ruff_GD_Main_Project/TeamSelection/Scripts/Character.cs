using Godot;
using System;

public class Character
{
    private int CharacterID;
    private string CharacterName;
    private string CharacterSprite;
    private string FullBodyCharacterSprite;
   

    public Character(int CharacterID, string CharacterName, string CharacterSprite, string FullBodyCharacterSprite)
    {
        this.CharacterID = CharacterID;
        this.CharacterName = CharacterName;
        this.CharacterSprite = CharacterSprite;
        this.FullBodyCharacterSprite = FullBodyCharacterSprite;
    }

    public int characterID
    {
        get { return CharacterID; }
        set { CharacterID = value; }
    }

    public string characterName
    {
        get { return CharacterName; }
        set { CharacterName = value; }
    }
    public string characterSprite
    {
        get { return CharacterSprite; }
        set { CharacterSprite = value; }
    }
    public string fullBodyCharacterSprite
    {
        get { return FullBodyCharacterSprite; }
        set { FullBodyCharacterSprite = value; }
    }
}
