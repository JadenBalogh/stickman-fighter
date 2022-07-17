using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Animation2D", menuName = "Animation2D", order = 400)]
public class Animation2D : ScriptableObject
{
    [SerializeField] private int frameRate;
    public int FrameRate { get => frameRate; }

    [SerializeField] private Sprite[] frames;
    public Sprite[] Frames { get => frames; }

    public float Duration { get => (float)frames.Length / frameRate; }
}
