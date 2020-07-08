using System;
using UnityEngine;
using System.Collections.Generic;


public class SigmoidalNetwork
{
    private SigmoidalNetwork(int inputCount, bool hasBias)      // Базовый конструктор класса (Задаёт количество входов нейросети) (BIAS в моей сети есть изначально)
    {
        if (inputCount <= 0)                                    // Если количество входов(каждого нейрона входного слоя) нейросети меньше или равно нулю
            Debug.LogError("The number of inputs of the neural network is zero, specify the correct number of inputs"); // Выдаём исключение
        InputCount = inputCount;                                // Присваиваем переменной "_inputCount" значение параметра метода "inputCount"
        HasBias = hasBias;                                      // Указываем есть ли у сети BIAS ()
    }

    /// <summary>Этот конструктор является вторым и наследуется от первого (базового). Он задаёт количество слоёв нейросети и количество нейронов в каждом слое.
    /// Этот конструктор не создаёт собственные нейроны и нейросеть, он принимает в качестве аргумента уже собранную нейросеть и нужен для загрузки созданной нейросети. </summary>
    /// <param name="inputCount"></param>
    /// <param name="layers"> Список массивов, где список это все слои нейронов, а каждый массив это слой с нейронами </param>
    public SigmoidalNetwork(int inputCount, bool hasBias, IList<NonLinearNeuron[]> layers) : this(inputCount, hasBias)
    {
        if (layers == null)                                                                 // Если список слоёв отсутствует
            Debug.LogError("layers");                                                       // Выдаём сообщение об ошибке
        if (layers.Count <= 0)                                                              // Если количество слоёв меньше или равно нулю
            Debug.LogError("No layers" + " layers");                                        // Выдаём сообщение об ошибке
        for (int i = 0; i < layers.Count; i++)                                              // Продолжаем цикл до тех пор пока не переберём все слои
            if (layers[i].Length <= 0)                                                      // Если количество нейронов в данном слое меньше или равно нулю
                Debug.LogError(string.Format("No neurons in layer {0}", i));                // Выдаём сообщение об ошибке
        Layers = layers;                                                                    // Задаёт все слои нейронов свойству "layers"
    }

    /// <summary> Этот конструктор создаёт слои и нейроны в нём по заданныи настройкам </summary>
    public SigmoidalNetwork(int inputCount, bool hasBias, int[] neuronCounts) : this(inputCount, hasBias)
    {
        Layers = new List<NonLinearNeuron[]>(neuronCounts.Length);                              // Создаём новый список слоёв нейронов, количество слоёв равно длинне массива neuronCounts
        for (int nLayer = 0; nLayer < neuronCounts.Length; nLayer++)                            // Продолжаем цикл пока не переберём все слои нейронов
        {
            if (neuronCounts[nLayer] <= 0)                                                      // Если в текущем слое нету нейронов
                Debug.LogError(string.Format("No neruons in layer {0}", nLayer));               // Выдаём сообщение об ошибке
            Layers.Add(new NonLinearNeuron[neuronCounts[nLayer]]);                              // Добавляем новый массив нелинейных нейронов, равный значению элемента neuronCounts
        }
    }

    /// <summary>Этот конструктор является третим в иерархии этого класса. Он заполняет пустые ячейки массива нейронов конкретным типом нейронов, с конкретным количеством входов,
    /// а так же добавляет дополнительный нейрон в этом текущем слое если предполагается использовать BIAS систему в нейросети</summary>
    public SigmoidalNetwork(int inputCount, bool hasBias, int[] neuronCounts, Type neuronType) : this(inputCount, hasBias, neuronCounts)
    {
        int biasInputs = hasBias ? 1 : 0;                                                       // Если в нейросети нужно использовать BIAS указываем значение int 1 иначе 0
        int neuronInputs = inputCount + biasInputs;                                             // В neuronInputs добавляем 1 нейрон если планируется использовать BIAS
        foreach (Neuron[] layer in Layers)                                                      // Перебираем в цикле слой в нейросети
        {
            for (int nNeuron = 0; nNeuron < layer.Length; nNeuron++)                            // Продолжаем цикл до тех пор пока не создадим все нейроны которые должны оказатся в этом слое
            {
                layer[nNeuron] = Activator.CreateInstance(neuronType, neuronInputs) as Neuron;  // Создаём новый нейрон с заданными параметрами и добавляем его в обрабатываемый слой
            }
            neuronInputs = layer.Length + biasInputs;                                           // Указываем количество входов для нового слоя как длинну текущего слоя + BIAS
        }
    }


    public int InputCount                   // Автсойство, количество входов(каждого нейрона входного слоя) нейросети
    { get; set; }                           // Задаёт и возвращает значения из автосвойства

    public bool HasBias                     // Автосвойство отображающая есть ли у сети BIAS (Параметр изменяемого свободного "порога" в нелинейных моделях (функциях))
    { get; set; }                           // Задаёт
    
    public IList<NonLinearNeuron[]> Layers  // Автосвойство, список массивов нейронов. Представляющий собой все слои и все нейроны в них
    { get; set; }

}
