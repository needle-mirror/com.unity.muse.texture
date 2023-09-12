using System;
using UnityEngine;

namespace Unity.Muse.Texture
{
    internal static class HdriProvider
    {
        internal static Cubemap GetHdri(HdriEnvironment environment)
        {
            return environment switch
            {
                HdriEnvironment.Default => Resources.Load<Cubemap>("HDRI/Tall Hall_lo"),
                HdriEnvironment.Inside => Resources.Load<Cubemap>("HDRI/IndoorEnvironmentHDRI002_4K-HDR"),
                HdriEnvironment.DayOutside => Resources.Load<Cubemap>("HDRI/DayEnvironmentHDRI030_4K-HDR"),
                HdriEnvironment.NightOutside => Resources.Load<Cubemap>("HDRI/NightEnvironmentHDRI002_4K-HDR"),
                HdriEnvironment.OutsideNeutral =>  Resources.Load<Cubemap>("HDRI/green_point_park_256_bw"),
                _ => throw new ArgumentOutOfRangeException(nameof(environment), environment, null)
            };
        }
    }
}