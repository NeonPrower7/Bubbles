using UnityEngine;

public class Door : MonoBehaviour
{
    private Animator animator; // Аниматор двери
    private Collider2D collider;

    private void Start()
    {
        animator = GetComponent<Animator>(); // Получаем компонент аниматора
        collider = GetComponent<BoxCollider2D>();
    }

    // Метод, который вызывается, когда другой объект входит в триггер коллайдера
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.transform.CompareTag("Player")) // Проверяем, что это игрок
        {
            PlayerInventory inventory = other.transform.GetComponent<PlayerInventory>();
            if (inventory != null && inventory.HasKey()) // Проверяем, есть ли у игрока ключ
            {
                inventory.UseKey(); // Используем ключ
                OpenDoor(); // Открываем дверь
            }
        }
    }

    // Метод для открытия двери
    private void OpenDoor()
    {
        animator.SetTrigger("Open"); // Запускаем анимацию открытия двери
        collider.enabled = false;
        Debug.Log("Дверь открыта!");
    }
}