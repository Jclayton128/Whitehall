using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Agent Data")]
public class AgentData : ScriptableObject
{
    public enum AgentAbility { None, Move, Search, Arrest, Pass}

    [SerializeField] string _agentName = "Default Agent";

    [SerializeField] Sprite _agentSprite = null;

    [SerializeField] AgentAbility[] _agentAbilities = null;

    public string AgentName => _agentName;
    public Sprite AgentSprite => _agentSprite;
    public AgentAbility[] AgentAbilities => _agentAbilities;

}
