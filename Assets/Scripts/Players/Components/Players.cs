// System
using System.Collections;
using System.Collections.Generic;

// Unity
using Unity.Mathematics;
using Unity.Entities;

namespace Players.Components
{
    public enum E_states
    {
        None = 0,
        Hovered,
        Selected
    }
    public enum E_role
    {
        DemiCentre = 0,
        Pivot,
        ArriereDroit,
        ArriereGauche,
        AilierDroit,
        AilierGauche,
        Count
    }
    public struct PlayersSettings : IComponentData
    {
        public E_role role;
    }

    public struct State : IComponentData
    {
        public E_states value;
    }

    namespace Tags
    {
        public struct HasBall : IComponentData { }
        public struct IsTeamA : IComponentData { }
        public struct IsTeamB : IComponentData { }
    }
}