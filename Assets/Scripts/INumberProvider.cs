using UnityEngine;
public interface INumberProvider
{
    Sprite[] numberSprites { get; }
    int nowNum { get; }
    int nextNum { get; }
    int nextNum2 { get; }
    int eraseCount { get; }
    int restoreCount { get; }
    int GetSpriteIndex(int number);
}