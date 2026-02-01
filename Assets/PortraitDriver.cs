using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PortraitDriver : MonoBehaviour
{
    //ref

    [SerializeField] Image _agentPortrait = null;
    [SerializeField] Image _ability_a0 = null;
    [SerializeField] Image _ability_a1 = null;
    [SerializeField] Image _ability_a2 = null;
    [SerializeField] Image _ability_b0 = null;
    [SerializeField] Image _ability_b1 = null;

    //settings
    Vector2 _smallScale = Vector2.one * 0.5f;
    float _turnOrderTweenTime = 0.5f;

    //state
    int _abilityCount = 0;

    public void SetPortrait(AgentData agentData)
    {
        _agentPortrait.sprite = agentData.AgentSprite;
        _agentPortrait.GetComponent<RectTransform>().localScale = _smallScale;

        _abilityCount = agentData.AgentAbilities.Length;

        _ability_a0.sprite = null;
        _ability_a1.sprite = null;
        _ability_a1.sprite = null;

        _ability_b0.sprite = null;
        _ability_b1.sprite = null;

        _ability_a0.enabled = false;
        _ability_a1.enabled = false;
        _ability_a2.enabled = false;

        _ability_b0.enabled = false;
        _ability_b1.enabled = false;

        if (_abilityCount == 0)
        {
            //must be the enemy

        }
        else if (_abilityCount == 2)
        {
            _ability_b0.sprite = GetAbilitySprite(agentData.AgentAbilities[0]);
            _ability_b1.sprite = GetAbilitySprite(agentData.AgentAbilities[1]);
        }
        else if (_abilityCount == 3)
        {
            _ability_a0.sprite = GetAbilitySprite(agentData.AgentAbilities[0]);
            _ability_a1.sprite = GetAbilitySprite(agentData.AgentAbilities[1]);
            _ability_a2.sprite = GetAbilitySprite(agentData.AgentAbilities[2]);
        }

  
    }

    private Sprite GetAbilitySprite(AgentData.AgentAbility ability)
    {
        if (ability == AgentData.AgentAbility.Move)
        {
            return ActorController.Instance.MoveAbilityIcon;
        }
        else if (ability == AgentData.AgentAbility.Search)
        {
            return ActorController.Instance.SearchAbilityIcon;
        }
        else return null;
    }

    public void EnlargePortrait()
    {
        _agentPortrait.GetComponent<RectTransform>().DOScale(Vector2.one, _turnOrderTweenTime);

        if (_abilityCount == 2)
        {
            _ability_b0.enabled = true;
            _ability_b1.enabled = true;
        }

        else if (_abilityCount == 3)
        {
            _ability_a0.enabled = true;
            _ability_a1.enabled = true;
            _ability_a2.enabled = true;
        }
    }

    public void ShrinkPortrait()
    {
        _agentPortrait.GetComponent<RectTransform>().DOScale(_smallScale, _turnOrderTweenTime);

        _ability_a0.enabled = false;
        _ability_a1.enabled = false;
        _ability_a2.enabled = false;

        _ability_b0.enabled = false;
        _ability_b1.enabled = false;

    }
}
