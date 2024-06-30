using MonkeDimensionsEditorTools;
using ExitGames.Client.Photon;
using Monke_Dimensions.API;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;
using Monke_Dimensions.Patches;

namespace Monke_Dimensions.Editor {
    public class MDCommand {
        public string Command = "";
        public List<char> GrabbedValueSpots = new List<char>();
    }

    public enum EventCode {
        BD = 98
    }

    public class MonkeDimensionsEditorHandler : MonoBehaviour, IOnEventCallback {
        public static MonkeDimensionsEditorHandler? instance;
        public GameObject MapObjectsParent;

        bool CanSendRpcs = true;

        void Awake() {
            if (instance is null)
                instance = this;
            else
                Destroy(this);

            PhotonNetwork.AddCallbackTarget(this);

            DimensionEvents.OnDimensionEnter += DimensionEntered;
            MDEvent += EventCalled;
        }

        void DimensionEntered(string DimensionName) {
            if (MapObjectsParent is null)
                MapObjectsParent = GameObject.Find("LoadedDimension");

            AllObjects = GetAllChildren(MapObjectsParent.transform);

            ApplyCommands();
        }

        void EventCalled(GameObject obj, int ID) {
            foreach (GameObject BDobj in AllObjects) {
                if (BDobj.GetComponent<MDEvent>() != null) {
                    MDEvent Event = BDobj.GetComponent<MDEvent>();
                    if (Event.EventID == ID) {
                        string[] MethodCommands = Event.ObjectCommands.Split('|');
                        foreach (string cmd in MethodCommands) {
                            GameObject objInMap = FindObjectInDimension(cmd);
                            if (objInMap.GetComponent<MDMethod>() is null) {
                                Debug.LogError($"Object {obj.name} recived an event and tried to execute a method but the method was not found");
                                break;
                            }

                            if (!Event.EventRan) {
                                RunCommands(objInMap.GetComponent<MDMethod>());
                                if (Event.type is MonkeDimensionsEditorTools.EventType.OneTrigger)
                                    Event.EventRan = true;
                            }
                        }
                    }
                }
            }
        }

        public List<GameObject> GetAllChildren(Transform parent) {
            List<GameObject> children = new List<GameObject>();
            GetChildrenLoop(parent, children);
            return children;
        }

        void GetChildrenLoop(Transform parent, List<GameObject> children) {
            foreach (Transform child in parent) {
                children.Add(child.gameObject);
                GetChildrenLoop(child, children);
            }
        }

        public List<MDCommand> Commands = new List<MDCommand>() {
            new MDCommand {
                Command = "debuglog/%",
                GrabbedValueSpots = new List<char> { '%' }
                //Grabbed value: debuglog/message
            },

            new MDCommand {
                Command = "debuglogwarning/%",
                GrabbedValueSpots = new List<char> { '%' }
                //Grabbed value: debuglogwarning/message
            },

            new MDCommand {
                Command = "debuglogerror/%",
                GrabbedValueSpots = new List<char> { '%' }
                //Grabbed value: debuglogerror/message
            },

            new MDCommand {
                Command = "runmethod/%",
                GrabbedValueSpots = new List<char> { '%' }
                //Grabbed value: runmethod/Gameobject with method
            },

            new MDCommand {
                Command = "if/%/*/&/^",
                GrabbedValueSpots = new List<char> { '%', '*', '&', '^' }
                //Grabbed value: if/Gameobject with variable/expectedValue/EventID if true/EventID if false
            },

            new MDCommand {
                Command = "ifnot/%/*/&/^",
                GrabbedValueSpots = new List<char> { '%', '*', '&', '^' }
                //Grabbed value: ifnot/Gameobject with variable/expectedValue/EventID if true/EventID if false
            },

            new MDCommand {
                Command = "changevar/%/*",
                GrabbedValueSpots = new List<char> { '%', '*' }
                //Grabbed value: changevar/Gameobject with variable/variable data to change to
            },

            new MDCommand {
                Command = "addtrigger/%/^",
                GrabbedValueSpots = new List<char> { '%', '^' }
                //Grabbed value: addtrigger/EventID/Trigger Type (left, right, both)
            },

            new MDCommand {
                Command = "setactive/*/%",
                GrabbedValueSpots = new List<char> { '*', '%' }
                //Grabbed value: setactive/gameobject name/bool
            },

            new MDCommand {
                Command = "sethitsound/%",
                GrabbedValueSpots = new List<char> { '%' }
                //Grabbed value: sethitsound/HitsoundID
            },

            new MDCommand {
                Command = "playaudio/%",
                GrabbedValueSpots = new List<char> { '%' }
                //Grabbed value: playaudio/Gameobject with audiosource
            },

            new MDCommand {
                Command = "starttimer/*/%",
                GrabbedValueSpots = new List<char> { '*', '%' }
                //Grabbed value: starttimer/EventID/Length in seconds
            },

            new MDCommand {
                Command = "stopaudio/%",
                GrabbedValueSpots = new List<char> { '%' }
                //Grabbed value: stopaudio/Gameobject with audiosource
            },

            new MDCommand {
                Command = "randomint/%/*/^/$",
                GrabbedValueSpots = new List<char> { '%', '*', '^', '$' }
                //Grabbed value: randomint/min/max/Gameobject with variable/Event ID
            },

            new MDCommand {
                Command = "settext/%/*",
                GrabbedValueSpots = new List<char> { '%', '*' }
                //Grabbed value: settext/newvalue/Gameobject with TMPro
            },

            new MDCommand {
                Command = "platform/%",
                GrabbedValueSpots = new List<char> { '%' }
                //Grabbed value: platform/Gameobject with animation
            },

            new MDCommand {
                Command = "addplayervelocity/%/*/$",
                GrabbedValueSpots = new List<char> { '%', '*', '$' }
                //Grabbed value: addplayervelocity/x/y/z
            },

            new MDCommand {
                Command = "setplayervelocity/%/*/$",
                GrabbedValueSpots = new List<char> { '%', '*', '$' }
                //Grabbed value: setplayervelocity/x/y/z
            },

            new MDCommand {
                Command = "rpc/%",
                GrabbedValueSpots = new List<char> { '%' }
                //Grabbed value: rpc/EventID
            },

            new MDCommand {
                Command = "add/%/*",
                GrabbedValueSpots = new List<char> { '%', '*' }
                //Grabbed value: add/GameObject with variable/by amount
                //adds the variable by the amount
            },

            new MDCommand {
                Command = "subtract/%/*",
                GrabbedValueSpots = new List<char> { '%', '*' }
                //Grabbed value: subtract/GameObject with variable/by amount
                //subtracts the variable by the amount
            },

            new MDCommand {
                Command = "multiply/%/*",
                GrabbedValueSpots = new List<char> { '%', '*' }
                //Grabbed value: multiply/GameObject with variable/by amount
                //multiplies the variable by the amount
            },

            new MDCommand {
                Command = "divide/%/*",
                GrabbedValueSpots = new List<char> { '%', '*' }
                //Grabbed value: divide/GameObject with variable/by amount
                //divides the variable by the amount
            },

            new MDCommand {
                Command = "setposition/%/*/^/$",
                GrabbedValueSpots = new List<char> { '%', '*', '^', '$' }
                //Grabbed value: setposition/GameObject/x/y/z
            },

            new MDCommand {
                Command = "setrotation/%/*/^/$",
                GrabbedValueSpots = new List<char> { '%', '*', '^', '$' }
                //Grabbed value: setrotation/GameObject/x/y/z
            },

            new MDCommand {
                Command = "setscale/%/*/^/$",
                GrabbedValueSpots = new List<char> { '%', '*', '^', '$' }
                //Grabbed value: setscale/GameObject/x/y/z
            },

            new MDCommand {
                Command = "setlocalposition/%/*/^/$",
                GrabbedValueSpots = new List<char> { '%', '*', '^', '$' }
                //Grabbed value: setlocalposition/GameObject/x/y/z
            },

            new MDCommand {
                Command = "setlocalrotation/%/*/^/$",
                GrabbedValueSpots = new List<char> { '%', '*', '^', '$' }
                //Grabbed value: setlocalrotation/GameObject/x/y/z
            },

            new MDCommand {
                Command = "playanimation/%/*",
                GrabbedValueSpots = new List<char> { '%', '*' }
                //Grabbed value: playanimation/GameObject with animator/AnimationState
            },

            new MDCommand {
                Command = "teleportplayer/%/*/&",
                GrabbedValueSpots = new List<char> { '%', '*', '&' }
                //Grabbed value: teleportplayer/z/y/z
            },
        };

        public Action<GameObject, int>? MDEvent;

        public List<GameObject> AllObjects = new List<GameObject>();

        void ApplyCommands() {
            foreach (GameObject obj in AllObjects) {
                if (!ObjectContainsCommands(obj))
                    continue;

                bool HasEventCMD = obj.GetComponent<MDEvent>() != null;
                bool HasMethodCMD = obj.GetComponent<MDMethod>() != null;
                bool HasVariableCMD = obj.GetComponent<MDVariable>() != null;

                int trueCount = (HasEventCMD ? 1 : 0) + (HasMethodCMD ? 1 : 0) + (HasVariableCMD ? 1 : 0);

                if (trueCount != 1) {
                    Debug.LogError($"Object {obj.name} can only have one BD component.");
                    Destroy(obj);
                    continue;
                }
                else {
                    if (obj.GetComponents<MDEvent>().Length > 1) {
                        Debug.LogError($"Object {obj.name} has more than one BDEvent component.");
                        Destroy(obj);
                        continue;
                    }
                    if (obj.GetComponents<MDMethod>().Length > 1) {
                        Debug.LogError($"Object {obj.name} has more than one BDMethod component.");
                        Destroy(obj);
                        continue;
                    }
                    if (obj.GetComponents<MDVariable>().Length > 1) {
                        Debug.LogError($"Object {obj.name} has more than one BDVariable component.");
                        Destroy(obj);
                        continue;
                    }
                }

                if (!HasMethodCMD)
                    return;

                MDMethod method = obj.GetComponent<MDMethod>();

                if (method.MethodType is MethodType.Awake)
                    RunCommands(method);
            }
        }

        public GameObject FindObjectInDimension(string ObjectName) {
            foreach (GameObject obj in AllObjects) {
                if (obj.name == ObjectName)
                    return obj;
            }

            return null;
        }

        public async void RunCommands(MDMethod method) {
            string[] MethodCommands = method.Commands.Split('|');

            foreach (string cmd in MethodCommands) {
                string[] Commandparts = cmd.Split('/');
                foreach (MDCommand command in Commands) {
                    string[] CheckParts = command.Command.Split('/');

                    //Template command
                    if (CheckParts[0] is "" && Commandparts[0] == CheckParts[0]) {
                        if (string.IsNullOrWhiteSpace(Commandparts[1])) {
                            Debug.LogError($"Object {method.gameObject.name} tried running \"\" but some values were empty");
                            break;
                        }

                        GameObject obj = FindObjectInDimension(Commandparts[1]);

                        if (obj is null || obj.GetComponent<MDMethod>() is null) {
                            Debug.LogError($"Object {method.gameObject.name} tried running \"\" but");
                            break;
                        }
                        break;
                    }
                    //Template command

                    if (CheckParts[0] is "debuglog" && Commandparts[0] == CheckParts[0]) {
                        if (string.IsNullOrWhiteSpace(Commandparts[1])) {
                            Debug.LogError($"Object {method.gameObject.name} tried running \"debuglog\" but put no message to log");
                            break;
                        }

                        string Message;

                        if (Commandparts[1].Contains("!VAR:")) {
                            string[] parts = Commandparts[1].Split(':');

                            GameObject obj = FindObjectInDimension(parts[1]);

                            MDVariable var = obj.GetComponent<MDVariable>();

                            if (obj is null || var is null) {
                                Debug.LogError("Method \"debuglog\" attempted variable interpolation but the object with the variable was null or didn't have a variable");
                                break;
                            }

                            Message = var.VariableData;
                        }
                        else
                            Message = Commandparts[1];

                        Debug.Log(Message);

                        break;
                    }

                    if (CheckParts[0] is "debuglogwarning" && Commandparts[0] == CheckParts[0]) {
                        if (string.IsNullOrWhiteSpace(Commandparts[1])) {
                            Debug.LogError($"Object {method.gameObject.name} tried running \"debuglogwarning\" but put no message to log");
                            break;
                        }

                        string Message;

                        if (Commandparts[1].Contains("!VAR:")) {
                            string[] parts = Commandparts[1].Split(':');

                            GameObject obj = FindObjectInDimension(parts[1]);

                            MDVariable var = obj.GetComponent<MDVariable>();

                            if (obj is null || var is null) {
                                Debug.LogError("Method \"debuglogwarning\" attempted variable interpolation but the object with the variable was null or didn't have a variable");
                                break;
                            }

                            Message = var.VariableData;
                        }
                        else
                            Message = Commandparts[1];

                        Debug.LogWarning(Message);

                        break;
                    }

                    if (CheckParts[0] is "debuglogerror" && Commandparts[0] == CheckParts[0]) {
                        if (string.IsNullOrWhiteSpace(Commandparts[1])) {
                            Debug.LogError($"Object {method.gameObject.name} tried running \"debuglogerror\" but put no message to log");
                            break;
                        }

                        string Message;

                        if (Commandparts[1].Contains("!VAR:")) {
                            string[] parts = Commandparts[1].Split(':');

                            GameObject obj = FindObjectInDimension(parts[1]);

                            MDVariable var = obj.GetComponent<MDVariable>();

                            if (obj is null || var is null) {
                                Debug.LogError("Method \"debuglogerror\" attempted variable interpolation but the object with the variable was null or didn't have a variable");
                                break;
                            }

                            Message = var.VariableData;
                        }
                        else
                            Message = Commandparts[1];

                        Debug.LogError(Message);

                        break;
                    }

                    if (CheckParts[0] is "runmethod" && Commandparts[0] == CheckParts[0]) {
                        if (string.IsNullOrWhiteSpace(Commandparts[1])) {
                            Debug.LogError($"Object {method.gameObject.name} tried running \"runmethod\" but put no GameObject method to run");
                            break;
                        }

                        GameObject TheObject;

                        if (Commandparts[1].Contains("!VAR:")) {
                            string[] parts = Commandparts[1].Split(':');

                            GameObject obj = FindObjectInDimension(parts[1]);

                            MDVariable var = obj.GetComponent<MDVariable>();

                            if (obj is null || var is null) {
                                Debug.LogError("Method \"runmethod\" attempted variable interpolation but the object with the variable was null or didn't have a variable");
                                break;
                            }

                            TheObject = FindObjectInDimension(var.VariableData);
                        }
                        else
                            TheObject = FindObjectInDimension(Commandparts[1]);

                        if (TheObject is null || TheObject.GetComponent<MDMethod>() is null) {
                            Debug.LogError($"Object {method.gameObject.name} tried running \"runmethod\" but put an invalid GameObject method to run");
                            break;
                        }

                        RunCommands(TheObject.GetComponent<MDMethod>());
                        break;
                    }

                    if (CheckParts[0] is "if" && Commandparts[0] == CheckParts[0]) {
                        if (string.IsNullOrWhiteSpace(Commandparts[1]) || string.IsNullOrWhiteSpace(Commandparts[2]) || string.IsNullOrWhiteSpace(Commandparts[3]) || string.IsNullOrWhiteSpace(Commandparts[4])) {
                            Debug.LogError($"Object {method.gameObject.name} tried running \"if\" some values were empty");
                            break;
                        }

                        GameObject obj = FindObjectInDimension(Commandparts[1]);

                        if (obj is null || obj.GetComponent<MDVariable>() is null) {
                            Debug.LogError($"Object {method.gameObject.name} tried running \"if\" but put an invalid GameObject variable to check");
                            break;
                        }

                        if (Commandparts[1] == Commandparts[2])
                            RunEvent(obj, int.Parse(Commandparts[3]));
                        else
                            RunEvent(obj, int.Parse(Commandparts[4]));

                        break;
                    }

                    if (CheckParts[0] is "ifnot" && Commandparts[0] == CheckParts[0]) {
                        if (string.IsNullOrWhiteSpace(Commandparts[1]) || string.IsNullOrWhiteSpace(Commandparts[2]) || string.IsNullOrWhiteSpace(Commandparts[3]) || string.IsNullOrWhiteSpace(Commandparts[4])) {
                            Debug.LogError($"Object {method.gameObject.name} tried running \"ifnot\" some values were empty");
                            break;
                        }

                        GameObject obj = FindObjectInDimension(Commandparts[1]);

                        if (obj is null || obj.GetComponent<MDVariable>() is null) {
                            Debug.LogError($"Object {method.gameObject.name} tried running \"ifnot\" but put an invalid GameObject variable to check");
                            break;
                        }

                        if (Commandparts[1] != Commandparts[2])
                            RunEvent(obj, int.Parse(Commandparts[3]));
                        else
                            RunEvent(obj, int.Parse(Commandparts[4]));

                        break;
                    }

                    if (CheckParts[0] is "changevar" && Commandparts[0] == CheckParts[0]) {
                        if (string.IsNullOrWhiteSpace(Commandparts[1]) || string.IsNullOrWhiteSpace(Commandparts[2])) {
                            Debug.LogError($"Object {method.gameObject.name} tried running \"changevar\" but put no GameObject variable to change");
                            break;
                        }

                        GameObject obj = FindObjectInDimension(Commandparts[1]);
                        MDVariable BDvar = obj.GetComponent<MDVariable>();

                        if (obj is null || BDvar is null) {
                            Debug.LogError($"Object {method.gameObject.name} tried running \"changevar\" but put an invalid GameObject variable to change");
                            break;
                        }

                        if (!BDvar.VariableChanged) {
                            BDvar.VariableData = Commandparts[2];

                            if (BDvar.Type == VariableType.OneChange)
                                BDvar.VariableChanged = true;
                        }
                        break;
                    }

                    if (CheckParts[0] is "addtrigger" && Commandparts[0] == CheckParts[0]) {
                        if (string.IsNullOrWhiteSpace(Commandparts[1]) || string.IsNullOrWhiteSpace(Commandparts[2])) {
                            Debug.LogError($"Object {method.gameObject.name} tried running \"addtrigger\" but some values were empty");
                            break;
                        }

                        EditorTrigger trigger = method.gameObject.AddComponent<EditorTrigger>();
                        trigger.ID = int.Parse(Commandparts[1]);

                        switch (Commandparts[2]) {
                            case "right":
                                trigger.Type = TriggerType.RightHand;
                                break;
                            case "left":
                                trigger.Type = TriggerType.LeftHand;
                                break;
                            case "both":
                                trigger.Type = TriggerType.BothHands;
                                break;
                        }
                        break;
                    }

                    if (CheckParts[0] is "setactive" && Commandparts[0] == CheckParts[0]) {
                        if (string.IsNullOrWhiteSpace(Commandparts[1]) || string.IsNullOrWhiteSpace(Commandparts[2])) {
                            Debug.LogError($"Object {method.gameObject.name} tried running \"setactive\" but some values were empty");
                            break;
                        }

                        GameObject TheObject;

                        if (Commandparts[1].Contains("!VAR:")) {
                            string[] parts = Commandparts[1].Split(':');

                            GameObject obj = FindObjectInDimension(parts[1]);

                            MDVariable var = obj.GetComponent<MDVariable>();

                            if (obj is null || var is null) {
                                Debug.LogError("Method \"setactive\" attempted variable interpolation but the object with the variable was null or didn't have a variable");
                                break;
                            }

                            TheObject = FindObjectInDimension(var.VariableData);
                        }
                        else
                            TheObject = FindObjectInDimension(Commandparts[1]);

                        if (TheObject is null) {
                            Debug.LogError($"Object {method.gameObject.name} tried running \"setactive\" on an object but that object didn't exist");
                            break;
                        }

                        if (Commandparts[2] is "true" || Commandparts[2] is "t")
                            TheObject.SetActive(true);
                        else if (Commandparts[2] is "false" || Commandparts[2] is "f")
                            TheObject.SetActive(false);
                        break;
                    }

                    if (CheckParts[0] is "sethitsound" && Commandparts[0] == CheckParts[0]) {
                        if (string.IsNullOrWhiteSpace(Commandparts[1])) {
                            Debug.LogError($"Object {method.gameObject.name} tried running \"sethitsound\" but some values were empty");
                            break;
                        }

                        if (method.gameObject.GetComponent<GorillaSurfaceOverride>() is null)
                            method.gameObject.AddComponent<GorillaSurfaceOverride>();

                        method.gameObject.GetComponent<GorillaSurfaceOverride>().overrideIndex = int.Parse(Commandparts[1]);
                        break;
                    }

                    if (CheckParts[0] is "playaudio" && Commandparts[0] == CheckParts[0]) {
                        if (string.IsNullOrWhiteSpace(Commandparts[1])) {
                            Debug.LogError($"Object {method.gameObject.name} tried running \"playaudio\" but some values were empty");
                            break;
                        }

                        GameObject TheObject;

                        if (Commandparts[1].Contains("!VAR:")) {
                            string[] parts = Commandparts[1].Split(':');

                            GameObject obj = FindObjectInDimension(parts[1]);

                            MDVariable var = obj.GetComponent<MDVariable>();

                            if (obj is null || var is null) {
                                Debug.LogError("Method \"playaudio\" attempted variable interpolation but the object with the variable was null or didn't have a variable");
                                break;
                            }

                            TheObject = FindObjectInDimension(var.VariableData);
                        }
                        else
                            TheObject = FindObjectInDimension(Commandparts[1]);

                        if (TheObject is null || TheObject.GetComponent<AudioSource>() is null) {
                            Debug.LogError($"Object {method.gameObject.name} tried running \"playaudio\" but the targeted GameObject didn't have an audio source or didn't exist");
                            break;
                        }

                        TheObject.GetComponent<AudioSource>().Play();
                        break;
                    }

                    if (CheckParts[0] is "stopaudio" && Commandparts[0] == CheckParts[0]) {
                        if (string.IsNullOrWhiteSpace(Commandparts[1])) {
                            Debug.LogError($"Object {method.gameObject.name} tried running \"stopaudio\" but some values were empty");
                            break;
                        }

                        GameObject TheObject;

                        if (Commandparts[1].Contains("!VAR:")) {
                            string[] parts = Commandparts[1].Split(':');

                            GameObject obj = FindObjectInDimension(parts[1]);

                            MDVariable var = obj.GetComponent<MDVariable>();

                            if (obj is null || var is null) {
                                Debug.LogError("Method \"stopaudio\" attempted variable interpolation but the object with the variable was null or didn't have a variable");
                                break;
                            }

                            TheObject = FindObjectInDimension(var.VariableData);
                        }
                        else
                            TheObject = FindObjectInDimension(Commandparts[1]);

                        if (TheObject is null || TheObject.GetComponent<AudioSource>() is null) {
                            Debug.LogError($"Object {method.gameObject.name} tried running \"stopaudio\" but the targeted GameObject didn't have an audio source or didn't exist");
                            break;
                        }

                        TheObject.GetComponent<AudioSource>().Stop();
                        break;
                    }

                    if (CheckParts[0] is "starttimer" && Commandparts[0] == CheckParts[0]) {
                        if (string.IsNullOrWhiteSpace(Commandparts[1]) || string.IsNullOrWhiteSpace(Commandparts[2])) {
                            Debug.LogError($"Object {method.gameObject.name} tried running \"starttimer\" but some values were empty");
                            break;
                        }

                        int TimeInSeconds;

                        if (Commandparts[1].Contains("!VAR:")) {
                            string[] parts = Commandparts[1].Split(':');

                            GameObject obj = FindObjectInDimension(parts[1]);

                            MDVariable var = obj.GetComponent<MDVariable>();

                            if (obj is null || var is null) {
                                Debug.LogError("Method \"starttimer\" attempted variable interpolation but the object with the variable was null or didn't have a variable");
                                break;
                            }

                            TimeInSeconds = int.Parse(var.VariableData);
                        }
                        else
                            TimeInSeconds = int.Parse(Commandparts[2]);

                        int EventID = int.Parse(Commandparts[1]);

                        await Task.Delay(TimeInSeconds * 1000);

                        RunEvent(method.gameObject, EventID);
                        break;
                    }

                    if (CheckParts[0] is "randomint" && Commandparts[0] == CheckParts[0]) {
                        if (string.IsNullOrWhiteSpace(Commandparts[1]) || string.IsNullOrWhiteSpace(Commandparts[2]) || string.IsNullOrWhiteSpace(Commandparts[3]) || string.IsNullOrWhiteSpace(Commandparts[4])) {
                            Debug.LogError($"Object {method.gameObject.name} tried running \"randomint\" but some values were empty");
                            break;
                        }

                        int randomValue = Random.Range(int.Parse(Commandparts[1]), int.Parse(Commandparts[2]));
                        GameObject obj = FindObjectInDimension(Commandparts[3]);

                        if (obj is null || obj.GetComponent<MDVariable>() is null) {
                            Debug.LogError($"Object {method.gameObject.name} tried running \"randomint\" and changing a variable on a gameobject but it didn't exist or didn't have a variable comp");
                            break;
                        }

                        obj.GetComponent<MDVariable>().VariableData = randomValue.ToString();
                        RunEvent(method.gameObject, int.Parse(Commandparts[4]));
                        break;
                    }

                    if (CheckParts[0] is "settext" && Commandparts[0] == CheckParts[0]) {
                        if (string.IsNullOrWhiteSpace(Commandparts[1]) || string.IsNullOrWhiteSpace(Commandparts[2])) {
                            Debug.LogError($"Object {method.gameObject.name} tried running \"settext\" but some values were empty");
                            break;
                        }

                        string Message;

                        if (Commandparts[1].Contains("!VAR:")) {
                            string[] parts = Commandparts[1].Split(':');

                            GameObject Obj = FindObjectInDimension(parts[1]);

                            MDVariable var = Obj.GetComponent<MDVariable>();

                            if (Obj is null || var is null) {
                                Debug.LogError("Method \"settext\" attempted variable interpolation but the object with the variable was null or didn't have a variable");
                                break;
                            }

                            Message = var.VariableData;
                        }
                        else
                            Message = Commandparts[1];

                        GameObject obj = FindObjectInDimension(Commandparts[2]);

                        if (obj is null || obj.GetComponent<TextMeshPro>() is null) {
                            Debug.LogError($"Object {method.gameObject.name} tried running \"settext\" but no objects were found with the TextMeshPro comp");
                            break;
                        }

                        obj.GetComponent<TextMeshPro>().text = Message;
                        break;
                    }

                    if (CheckParts[0] is "platform" && Commandparts[0] == CheckParts[0]) {
                        if (string.IsNullOrWhiteSpace(Commandparts[1])) {
                            Debug.LogError($"Object {method.gameObject.name} tried running \"platform\" but some values were empty");
                            break;
                        }

                        GameObject obj = FindObjectInDimension(Commandparts[1]);

                        if (obj is null || obj.GetComponent<TextMeshPro>() is null) {
                            Debug.LogError($"Object {method.gameObject.name} tried running \"platform\" but no objects were found with the TextMeshPro comp");
                            break;
                        }

                        if (obj.GetComponent<MovingPlatform>() is null)
                            obj.AddComponent<MovingPlatform>();

                        break;
                    }

                    if (CheckParts[0] is "addplayervelocity" && Commandparts[0] == CheckParts[0]) {
                        if (string.IsNullOrWhiteSpace(Commandparts[1]) || string.IsNullOrWhiteSpace(Commandparts[2]) || string.IsNullOrWhiteSpace(Commandparts[3])) {
                            Debug.LogError($"Object {method.gameObject.name} tried running \"addplayervelocity\" but some values were empty");
                            break;
                        }

                        Vector3 velocity = new Vector3(float.Parse(Commandparts[1]), float.Parse(Commandparts[2]), float.Parse(Commandparts[3]));

                        GorillaTagger.Instance.rigidbody.AddForce(velocity);
                        break;
                    }

                    if (CheckParts[0] is "setplayervelocity" && Commandparts[0] == CheckParts[0]) {
                        if (string.IsNullOrWhiteSpace(Commandparts[1]) || string.IsNullOrWhiteSpace(Commandparts[2]) || string.IsNullOrWhiteSpace(Commandparts[3])) {
                            Debug.LogError($"Object {method.gameObject.name} tried running \"addplayervelocity\" but some values were empty");
                            break;
                        }

                        Vector3 velocity = new Vector3(float.Parse(Commandparts[1]), float.Parse(Commandparts[2]), float.Parse(Commandparts[3]));

                        GorillaTagger.Instance.rigidbody.velocity = velocity;
                        break;
                    }

                    if (CheckParts[0] is "rpc" && Commandparts[0] == CheckParts[0]) {
                        if (string.IsNullOrWhiteSpace(Commandparts[1])) {
                            Debug.LogError($"Object {method.gameObject.name} tried running \"rpc\" but some values were empty");
                            break;
                        }

                        if (!CanSendRpcs) {
                            Debug.LogWarning("Rpc sending is on cooldown");
                            break;
                        }

                        if (Commandparts[1].ToString().Length > 5) {
                            Debug.LogWarning("The sent data in a MonkeDimensions rpc cannot be higher then 5 charaters");
                            return;
                        }

                        int EventID = int.Parse(Commandparts[1]);

                        if (PhotonNetwork.PlayerList.Length is 1) {
                            RunEvent(method.gameObject, EventID);
                            break;
                        }

                        PhotonNetwork.RaiseEvent((byte)EventCode.BD, new object[] { EventID }, new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);

                        CanSendRpcs = false;

                        await Task.Delay(3000);

                        CanSendRpcs = true;

                        break;
                    }

                    if (CheckParts[0] is "add" && Commandparts[0] == CheckParts[0]) {
                        if (string.IsNullOrWhiteSpace(Commandparts[1])) {
                            Debug.LogError($"Object {method.gameObject.name} tried running \"add\" but some values were empty");
                            break;
                        }

                        GameObject obj = FindObjectInDimension(Commandparts[1]);

                        if (obj is null || obj.GetComponent<MDMethod>() is null) {
                            Debug.LogError($"Object {method.gameObject.name} tried running \"add\" but the GameObject with variable could not be found");
                            break;
                        }

                        MDVariable Var = obj.GetComponent<MDVariable>();

                        int PrevInt = int.Parse(Var.VariableData);

                        PrevInt += int.Parse(Commandparts[2]);
                        Var.VariableData = PrevInt.ToString();
                        break;
                    }

                    if (CheckParts[0] is "subtract" && Commandparts[0] == CheckParts[0]) {
                        if (string.IsNullOrWhiteSpace(Commandparts[1])) {
                            Debug.LogError($"Object {method.gameObject.name} tried running \"add\" but some values were empty");
                            break;
                        }

                        GameObject obj = FindObjectInDimension(Commandparts[1]);

                        if (obj is null || obj.GetComponent<MDMethod>() is null) {
                            Debug.LogError($"Object {method.gameObject.name} tried running \"add\" but the GameObject with variable could not be found");
                            break;
                        }

                        MDVariable Var = obj.GetComponent<MDVariable>();

                        int PrevInt = int.Parse(Var.VariableData);

                        PrevInt += int.Parse(Commandparts[2]);
                        Var.VariableData = PrevInt.ToString();
                        break;
                    }

                    if (CheckParts[0] is "setposition" && Commandparts[0] == CheckParts[0]) {
                        if (string.IsNullOrWhiteSpace(Commandparts[1]) || string.IsNullOrWhiteSpace(Commandparts[2]) || string.IsNullOrWhiteSpace(Commandparts[3]) || string.IsNullOrWhiteSpace(Commandparts[4])) {
                            Debug.LogError($"Object {method.gameObject.name} tried running \"setposition\" but some values were empty");
                            break;
                        }

                        GameObject obj = FindObjectInDimension(Commandparts[1]);

                        if (obj is null) {
                            Debug.LogError($"Object {method.gameObject.name} tried running \"setposition\" but the object to set is null");
                            break;
                        }

                        obj.transform.position = new Vector3(float.Parse(Commandparts[1]), float.Parse(Commandparts[2]), float.Parse(Commandparts[3]));
                        break;
                    }

                    if (CheckParts[0] is "setrotation" && Commandparts[0] == CheckParts[0]) {
                        if (string.IsNullOrWhiteSpace(Commandparts[1]) || string.IsNullOrWhiteSpace(Commandparts[2]) || string.IsNullOrWhiteSpace(Commandparts[3]) || string.IsNullOrWhiteSpace(Commandparts[4])) {
                            Debug.LogError($"Object {method.gameObject.name} tried running \"setrotation\" but some values were empty");
                            break;
                        }

                        GameObject obj = FindObjectInDimension(Commandparts[1]);

                        if (obj is null) {
                            Debug.LogError($"Object {method.gameObject.name} tried running \"setrotation\" but the object to set is null");
                            break;
                        }

                        obj.transform.rotation = Quaternion.Euler(float.Parse(Commandparts[1]), float.Parse(Commandparts[2]), float.Parse(Commandparts[3]));
                        break;
                    }

                    if (CheckParts[0] is "setscale" && Commandparts[0] == CheckParts[0]) {
                        if (string.IsNullOrWhiteSpace(Commandparts[1]) || string.IsNullOrWhiteSpace(Commandparts[2]) || string.IsNullOrWhiteSpace(Commandparts[3]) || string.IsNullOrWhiteSpace(Commandparts[4])) {
                            Debug.LogError($"Object {method.gameObject.name} tried running \"setscale\" but some values were empty");
                            break;
                        }

                        GameObject obj = FindObjectInDimension(Commandparts[1]);

                        if (obj is null) {
                            Debug.LogError($"Object {method.gameObject.name} tried running \"setscale\" but the object to set is null");
                            break;
                        }

                        obj.transform.localScale = new Vector3(float.Parse(Commandparts[1]), float.Parse(Commandparts[2]), float.Parse(Commandparts[3]));
                        break;
                    }

                    if (CheckParts[0] is "setlocalposition" && Commandparts[0] == CheckParts[0]) {
                        if (string.IsNullOrWhiteSpace(Commandparts[1]) || string.IsNullOrWhiteSpace(Commandparts[2]) || string.IsNullOrWhiteSpace(Commandparts[3]) || string.IsNullOrWhiteSpace(Commandparts[4])) {
                            Debug.LogError($"Object {method.gameObject.name} tried running \"setlocalposition\" but some values were empty");
                            break;
                        }

                        GameObject obj = FindObjectInDimension(Commandparts[1]);

                        if (obj is null) {
                            Debug.LogError($"Object {method.gameObject.name} tried running \"setlocalposition\" but the object to set is null");
                            break;
                        }

                        obj.transform.localPosition = new Vector3(float.Parse(Commandparts[1]), float.Parse(Commandparts[2]), float.Parse(Commandparts[3]));
                        break;
                    }

                    if (CheckParts[0] is "setlocalrotation" && Commandparts[0] == CheckParts[0]) {
                        if (string.IsNullOrWhiteSpace(Commandparts[1]) || string.IsNullOrWhiteSpace(Commandparts[2]) || string.IsNullOrWhiteSpace(Commandparts[3]) || string.IsNullOrWhiteSpace(Commandparts[4])) {
                            Debug.LogError($"Object {method.gameObject.name} tried running \"setlocalrotation\" but some values were empty");
                            break;
                        }

                        GameObject obj = FindObjectInDimension(Commandparts[1]);

                        if (obj is null) {
                            Debug.LogError($"Object {method.gameObject.name} tried running \"setlocalrotation\" but the object to set is null");
                            break;
                        }

                        obj.transform.localRotation = Quaternion.Euler(float.Parse(Commandparts[1]), float.Parse(Commandparts[2]), float.Parse(Commandparts[3]));
                        break;
                    }

                    if (CheckParts[0] is "playanimation" && Commandparts[0] == CheckParts[0]) {
                        if (string.IsNullOrWhiteSpace(Commandparts[1]) || string.IsNullOrWhiteSpace(Commandparts[2])) {
                            Debug.LogError($"Object {method.gameObject.name} tried running \"playanimation\" but some values were empty");
                            break;
                        }

                        string AnimationState;

                        if (Commandparts[1].Contains("!VAR:")) {
                            string[] parts = Commandparts[1].Split(':');

                            GameObject Obj = FindObjectInDimension(parts[1]);

                            MDVariable var = Obj.GetComponent<MDVariable>();

                            if (Obj is null || var is null) {
                                Debug.LogError("Method \"playaudio\" attempted variable interpolation but the object with the variable was null or didn't have a variable");
                                break;
                            }

                            AnimationState = var.VariableData;
                        }
                        else
                            AnimationState = Commandparts[1];

                        GameObject obj = FindObjectInDimension(Commandparts[1]);

                        if (obj is null) {
                            Debug.LogError($"Object {method.gameObject.name} tried running \"playanimation\" but the object to set is null");
                            break;
                        }

                        obj.GetComponent<Animator>().Play(AnimationState);
                        break;
                    }

                    if (CheckParts[0] is "teleportplayer" && Commandparts[0] == CheckParts[0]) {
                        if (string.IsNullOrWhiteSpace(Commandparts[1]) || string.IsNullOrWhiteSpace(Commandparts[2]) || string.IsNullOrWhiteSpace(Commandparts[3])) {
                            Debug.LogError($"Object {method.gameObject.name} tried running \"teleportplayer\" but some values were empty");
                            break;
                        }

                        TeleportPatch.TeleportPlayer(new Vector3(float.Parse(Commandparts[1]), float.Parse(Commandparts[2]), float.Parse(Commandparts[3])), 0f, true);
                        break;
                    }
                }
            }
        }

        public void RunEvent(GameObject obj, int EventID) =>
            MDEvent?.Invoke(obj, EventID);

        public bool ObjectContainsCommands(GameObject obj) {
            if (obj.GetComponent<MDEvent>() != null || obj.GetComponent<MDMethod>() != null || obj.GetComponent<MDVariable>() != null)
                return true;

            return false;
        }

        public void OnEvent(EventData photonEvent) {
            if (photonEvent.Code != 98)
                return;

            object[] data = (object[])photonEvent.CustomData;

            RunEvent(gameObject, (int)data[0]);
        }
    }
}