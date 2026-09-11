using System;
using UnityEngine;
using Deucarian.Combat;
namespace Deucarian.Combat.Unity.Samples.DefinitionWorkflow
{
    /// <summary>Small caller example. The configured scene hosts own services and resource lifetimes.</summary>
    public sealed class CombatWorkflow : MonoBehaviour
    {
        [SerializeField] private Combatant target;
        [SerializeField] private DamageTypeKey damage;
        [SerializeField] private DamageTrigger trigger;
        private string status = "Ready. Choose an action below.";
        public string Status => status;
        public void Damage() { target.Host.ApplyDamage(target.Handle, damage, 10); status = "Health: " + target.CurrentHealth; }
        public void DamageComponent() { trigger.Apply(); status = "Health: " + target.CurrentHealth; }
        public void Reactivate() { target.enabled = false; target.enabled = true; status = "Health retained: " + target.CurrentHealth; }
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(24, 24, Math.Min(540, Screen.width - 48), Screen.height - 48), GUI.skin.box);
            GUILayout.Label("Combat — definition workflow");
            GUILayout.Label("A Combatant owns this actor's health. The shared CombatHost resolves typed damage; disabling and enabling the actor retains its health.");
            GUILayout.Space(12);
            if (GUILayout.Button("Apply damage with C#", GUILayout.Height(32))) { try { Damage(); } catch (Exception error) { status = error.Message; } }
            if (GUILayout.Button("Apply damage with component", GUILayout.Height(32))) { try { DamageComponent(); } catch (Exception error) { status = error.Message; } }
            if (GUILayout.Button("Disable / enable actor", GUILayout.Height(32))) { try { Reactivate(); } catch (Exception error) { status = error.Message; } }
            GUILayout.Space(12);
            GUILayout.Label(status);
            GUILayout.EndArea();
        }
    }
}
