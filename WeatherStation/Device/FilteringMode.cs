﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace WeatherStation.Device
{
    /// <summary>
    /// Bmx280 devices feature an internal IIR filter.
    /// This filter effectively reduces the bandwidth of the temperature and pressure output signals
    /// and increases the resolution of the pressure and temperature output data to 20 bits.
    ///
    /// The higher the coefficient, the slower the sensors respond to external inputs.
    ///
    /// See the data sheet with recommended settings for different scenarios.
    /// </summary>
    public enum FilteringMode : byte
    {
        /// <summary>
        /// Filter off
        /// </summary>
        Off = 0b000,
        /// <summary>
        /// Coefficient x2
        /// </summary>
        X2 = 0b001,
        /// <summary>
        /// Coefficient x4
        /// </summary>
        X4 = 0b010,
        /// <summary>
        /// Coefficient x8
        /// </summary>
        X8 = 0b011,
        /// <summary>
        /// Coefficient x16
        /// </summary>
        X16 = 0b100
    }
}
