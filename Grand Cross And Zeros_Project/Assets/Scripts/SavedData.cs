// This class is not attached to more than one object, but is serialized and is used to save data to a binary file.
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Runtime.Serialization;


[Serializable]
public class SavedData
{
    public byte Progress;                                           // The number of the last passed level is stored here.
    public List<int> NumberMapsScreenshots;                         // Numbers of screenshots of maps are stored here.
    public int NumLastScreen;                                       // Number of last saved screenshot
    public Levels CustomLevels = new Levels();                      // User level data is stored here.
}


[Serializable]
public class SerializableVector3 : ISerializable   // Еhe class that implements the ISerializable interface is needed to replace the usual vector3 and serialize it
{
    Vector3 vector;                         // Variable normal vector3

    public Vector3 GetPoint                 // The property allows you to get the value of the field "vector3"
    {
        get
        {
            return vector;
        }
    }


    public SerializableVector3(Vector3 v)   // The class constructor determines the value of vector 3
    {
        this.vector = v;                    // The Vector3 argument passed to the constructor initializes the value of the field named "vector"
    }

    public SerializableVector3(SerializationInfo info, StreamingContext context)    // Overloaded constructor of class "SerializableVector3" which retrieves data from the storage and assigns them to the field vector
    {
        float x = (float)info.GetValue("x", typeof(float));                         // We retrieve the value of "x" from the storage and convert it to the type float
        float y = (float)info.GetValue("y", typeof(float));                         // We retrieve the value of "y" from the storage and convert it to the type float
        float z = (float)info.GetValue("z", typeof(float));                         // We retrieve the value of "x" from the storage and convert it to the type float
        vector = new Vector3(x, y, z);                                              // Assigning the variable "vector" values extracted from the repository
    }

    // The method that sets the data in the repository, (This method must exist as it is declared in the iSializable interface that implements this class "SerializableVector3")
    // A class is called to get data but it add it. Most likely this is a mistake, but it was just such an example that they wrote me to parse the code, in the future you can change.
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("x", vector.x);                               // Add the x value from the vector field to the storage          
        info.AddValue("y", vector.y);                               // Add the y value from the vector field to the storage
        info.AddValue("z", vector.z);                               // Add the z value from the vector field to the storage 
    }

    public static implicit operator Vector3(SerializableVector3 p)              // Operator overload, user-defined operator type implicitly converts user-defined point class to vector3
    {
        return p.vector;                                                        // Specify the conversion from point to vector3
    }

    public static implicit operator SerializableVector3(Vector3 v)              // Operator overloading, a custom operator type implicitly converts a class vector3 into a serializable vector3
    {
        return new SerializableVector3(v);                                      // Specify the conversion from vector3 to serializable vector3
    }
}

[Serializable]
public class Levels                                                         // Class level, it contains a list of Levels
{
    public List<Level> LevelList = new List<Level>();                       // LevelList list
}

[Serializable]
public class Level                                                          // Class level, it contains a list of lines
{
    public List<Line> Lines = new List<Line>();                             // Line list
    public List<CellGroup> CellGroup = new List<CellGroup>();               // List of fog cell groups
}

[Serializable]
public class Line                                                               // Class line, it contains a list of serializable vector3
{
    public List<SerializableVector3> Points = new List<SerializableVector3>();  // List of serializable vector3

    public void AddRange(List<Vector3> InputVector3List)                        // This method adds a list of points of type "vector3" points to a list of type "Points"
    {
        for (int a = 0; a < InputVector3List.Count; a++)
        {
            Points.Add(InputVector3List[a]);
        }
    }
}

[Serializable]
public class CellGroup                                                      // This class stores a group of fog cells.
{
    public List<FogCell> Cells = new List<FogCell>();                       // Cells list
    public GameManager.CellGroupTypes GroupType;                            // Type of fog cell group
}

[Serializable]
public class FogCell                                                        // This is a class of fog cell that preserves its position and type.
{
    public SerializableVector3 Point;                                       // Stored fog cell position
    public GameManager.FogCellTypes FogType;                                // Fog cell type
}














