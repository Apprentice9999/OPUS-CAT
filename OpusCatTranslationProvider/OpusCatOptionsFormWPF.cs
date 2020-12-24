﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpusCatTranslationProvider
{
    public partial class OpusCatOptionsFormWPF : Form
    {
        public OpusCatOptionsFormWPF(OpusCatOptions options, Sdl.LanguagePlatform.Core.LanguagePair[] languagePairs)
        {
            this.Options = options;
            InitializeComponent();
            this.wpfHost.Child = new OpusCatOptionControl(this, options, languagePairs);
        }

        public OpusCatOptions Options { get; internal set; }

    }
}
