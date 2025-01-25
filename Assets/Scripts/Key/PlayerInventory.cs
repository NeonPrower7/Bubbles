using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private int keyCount = 0; // Количество ключей в инвентаре

    // Метод для добавления ключа в инвентарь
    public void AddKey()
    {
        keyCount++;
    }

    // Метод для проверки наличия ключа
    public bool HasKey()
    {
        return keyCount > 0;
    }

    // Метод для использования ключа
    public void UseKey()
    {
        if (keyCount > 0)
        {
            keyCount--;
        }
    }
}