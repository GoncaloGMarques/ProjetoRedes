﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Modelos
{
    public enum EstadoJogo
    {
        ConnectionOpen,
        ConnectionClosed,
        JogoInitializing,
        JogoPlacingBoats,
        JogoStarted,
        JogoEnded
    }
}