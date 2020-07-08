using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class NonLinearNeuron : Neuron                      // This class describes a non-linear neuron.
{
    public NonLinearNeuron(int inputCount) : base(inputCount) { }   // When calling the constructor of this class, we pass its parameter further to the base class "Neuron" as an argument

    public abstract double ActivationFunction(double arg);          // Abstract neuron activation method with arg parameter

    public override double Response(double[] InputSignals)          // Redefining the Response method (neuron response) from the base class "Neuron"
    {
        // We call the base method "Response" of the class "Neuron" By sending the argument "InputSignals" to it and when it returns the answer,
        // then we send this answer as an argument by calling the abstract method "ActivationFunction" which will be described only in the derived class.
        return ActivationFunction(base.Response(InputSignals));
    }

    // Widrow-Hoff Neuron Training
    public void LearnWidrowHoff(double[] signals, double expectedOutput, double ratio, out double previousResponse, out double previousError)
    {   // Take the linear answer into account.
        previousResponse = base.Response(signals);                  // We call the "Response" method of the base class which returns the response of the neuron and returns it from the method
        previousError = expectedOutput - previousResponse;          // We calculate the neuron error and return it from the method
        for (int i = 0; i < Weights.Length; i++)                    // We continue the cycle until we sort through all the weights of this neuron interrogated in the method
            Weights[i] += ratio * previousError * signals[i];       // We change the weight of the neuron, thereby training it
    }
}
