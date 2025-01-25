using UnityEngine;

public class PlayerFollowMouse2D : MonoBehaviour
{
    void Update()
    {
        // Получаем положение курсора мыши в мировых координатах
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0; // Устанавливаем Z координату в 0, так как мы работаем в 2D

        // Вычисляем направление от игрока к курсору
        Vector3 direction = mousePosition - transform.position;
        
        // Вычисляем угол поворота в радианах и конвертируем его в градусы
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Устанавливаем угол поворота игрока
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }
}