using System.Collections.Generic;

namespace HarmonyMod
{
    internal class AccessControlLists
    {
        /* Individuals who in some ways shit on the any community I am in
         * and seed discord and division are not permitted to copy or run
         * this software, by virtue of the LICENSE.
         *
         * Their primary steam ID's are listed here.
         *
         * The implementation of this access control list is a lock under
         * DMCA legislation
         */

        static public HashSet<ulong> assholes = new HashSet<ulong>()
        {
            76561198855893485,
        76561198097535939,
        76561198027494461,
        76561199126305901,
        76561198449029071,
        76561198262198841,
        76561198109315306,
        76561198035630804,
        76561198322250977,
        76561197968340476,
        76561197968592937,
        76561198007746943,
        76561198063330220,
        76561198110157252,
        76561197983491560,
        76561198866403662,
        76561197991343677,
        76561198203183750,
        76561198012466485,
        76561198029530860,
        76561197992653878,
        76561198034391960,
        76561197960468888,
        76561198031588936,
        76561198174114409,
        76561198874236932,
        76561198373219996,
        76561198040139417,
        76561198268495615,
        76561198049116461,
        76561198049116461,
        76561198158407437,
        76561198320564937,
        76561198031001669,

        };

        static public HashSet<ulong> trolls = new HashSet<ulong>()
        {
            76561197962306884,
            76561198017937996,
        };

        /* Useful tools:
         *
         * https://steamdb.info/calculator/76561198449029071/
         * https://steamid.io/lookup/76561198268495615
         */
        public bool isBlocked(){
            return assholes.Contains(PlatformService.userID.AsUInt64) ||
                trolls.Contains(PlatformService.userID.AsUInt64);
        }
    };

}
