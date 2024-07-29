using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDragBehaviour
{
    public void SetOnConfig(bool onConfig);
    public void SetConfigState(int state);
    public void ResetToInitial();
    public void MoveXZ();
    public void MoveY();
    public void Rotate();
    public void Scale();
}