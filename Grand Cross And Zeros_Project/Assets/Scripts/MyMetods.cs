using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MyMetods
{
    private static bool  Forward;           // Эта переменная определяет двигаемся мы вперёд - от меньшего к большему числу или назад от большего к меньшему
    private static float Difference;        // Разница в значении между началом и концом 
    private static float OnePercent;        // Один процент
 

    public static float Lerp(float Start, float Real, float End, float Percent/*, ref float PercentComplete*/)
    {
        Forward = false;                                // Указываем что сначала нам не известо что больше "Старт" или "Конец"
        if (Start < End) Forward = true;                // Если значение в переменной старт меньше чем значение в переменной конец, значит указываем что мы будем двигать панель назад
        Difference = End - Start;                       // Узнаём разницу между начальным и конечным значением
        OnePercent = Difference / 100;                  // Узнаём чему равен 1 процент и сохраняем это значение
        Real = ((OnePercent * Percent) + Real);         // Прибавляем к текущему значению значение в процетах указанное в переменной Percent
       
        if (Forward)                                    // Если мы двигаемся от меньшего числа к большему
        {
            if (Real > End) Real = End;                 // Если реальная позиция больше  конечного значения присваиваем реальной позиции конечную позицию
        }
        else                                            // Иначе если мы двигаемся от большего числа к меньшему
        {
            if (Real < End) Real = End;                 // Если реальная позиция меньше конченого значения присваиваем реальной позиции конечную позицию
        }

        return Real;                                    // Возвращаем новую реальную позицию
    }
}
