using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private int keyCount = 0; // Количество ключей в инвентаре

    // Метод для добавления ключа в инвентарь
    public void AddKey()
    {
        keyCount++;
        Debug.Log("You have" + keyCount + "keys");
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
            Debug.Log("Ключ использован! Оставшееся количество ключей: " + keyCount);
        }
    }
}