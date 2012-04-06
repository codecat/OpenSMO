using System;

namespace OpenSMO {
    // For more information, plase see the following article:
    // https://github.com/freem/SMOnline-v1/wiki/Protocol

    public enum NSCommand {     //         Client -> Server      Server -> Client
        //                      //  -----------------------------------------------
        NSCPing = 0,            //  0 --> Ping                | ~
        NSCPingR,               //  1 --> Ping Response       | ~
        NSCHello,               //  2 --> Handshake           | ~
        NSCGSR,                 //  3 --> Game start request  | ~
        NSCGON,                 //  4 --> Game over notice    | ~
        NSCGSU,                 //  5 --> Game status update  | Scoreboard update
        NSCSU,                  //  6 --> Style update        | System message
        NSCCM,                  //  7 --> Chat message        | ~
        NSCRSG,                 //  8 --> Request start game  | Ask client for song or start song
        NSCUUL,                 //  9 --> Reserved            | Update user list
        NSCSMS,                 // 10 --> Screen changed      | Force ScreenNetSelectMusic
        NSCUPOpts,              // 11 --> Player options      | Reserved
        NSCSMOnline,            // 12 --> SMOnline            | ~
        NSCFormatted,           // 13 --> Reverved            | Server information
        NSCAttack,              // 14 --> Reserved            | Attack client
        NSCLUA,                 // 15 --> ~                   | a-smop4 Lua eval
        NUM_NS_COMMANDS
    }
}
