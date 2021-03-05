using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Card : NetworkBehaviour
{
    [SyncVar] public string ownerPlayerName;
    [SyncVar] public int ownerConnectionId;
    [SyncVar] public int ownerPlayerNumber;

    public string CardName;
    public int Power;
    public int AttackValue;
    public int DefenseValue;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
