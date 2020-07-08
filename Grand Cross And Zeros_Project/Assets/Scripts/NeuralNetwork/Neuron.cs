using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Neuron
{
    double[] _weights;      // This array stores the values of the input neuron weights.
    double[] _prevWeights;  // This array stores the previous input weights of this neuron.

    // Constructor class neuron. Creates a neuron containing the specified number of inputs passed to the "InputCount" parameter.
    // The array of weights will have a size equal to the specified number of inputs
    public Neuron(int InputCount)
    {
        _weights = new double[InputCount];      // The size of the array of weights is determined by the input argument
        _prevWeights = new double[InputCount];  // The size of the array of previous weights is determined by the input argument
    }

    public double[] Weights         // Returns an array containing the weights of each of this neuron's inputs.
    {
        get
        {
            return _weights;        // Return an array of neuron entry weights
        }
    }

    public double[] PrevWeights     // Property for an array containing the previous weights of each input of this neuron.
    {
        get
        {
            return _prevWeights;    // Return an array of previous neuron entry weights
        }
    }

    // InputSignals - These are signals received at the inputs of a neuron.
    // Result returns the response of a neuron to a given signal.
    // To calculate the response of a neuron, we need to calculate the product of each component of the input signal and the corresponding input weight.
    // The sum of these products is our answer. In practice, it turns out that the more the signal is “similar” to the scales memorized by the neuron, the more positive the response of the neuron will be.
    // Each element of the array corresponds to a separate input, so the length array must calculate the number of inputs
    /// <summary> Returns the result of computing a neuron for input signals, that is, its solution. </summary> 
    public virtual double Response(double[] InputSignals)
    {
        if (InputSignals == null || InputSignals.Length != _weights.Length) // Check for correct input.
            Debug.LogError("The signal array must have the same length as the weight array.");

        double result = 0.0;                            // We calculate the output as the sum of numbers.

        for (int i = 0; i < _weights.Length; i++)       // We continue the cycle until we sort through all incoming normalized signals
            result += _weights[i] * InputSignals[i];    // We multiply each normalized signal by its weight and add the result to the variable result

        return result;      // We return the result from the method
    }
}
