using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFComputer.Services;


public sealed class ComputerSystemService {
    private static readonly Lazy<ComputerSystemService> lazy =
        new(() => new ComputerSystemService());

    public static ComputerSystemService Instance => lazy.Value;

    private ComputerSystemService() {
    }

    //TO DO: implement state management
    //TO DO: implement threaded run operation
    //TO DO: implement peripheral attachments
    //TO DO: implement global events

    /*
     run
     pause
     resume (=run)
     off
     on
     halt?
     load
     loadandrun
     runmonitor
     enablehostenhancements(bool)
     attachsound
     attachtty
     attachconsole
     attachrastergraphics(mode=default)
     savecheckpoint
     restorecheckpoint
     
     */
}