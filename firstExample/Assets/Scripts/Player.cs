using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Player : NetworkBehaviour
{
    //Varíável de sincronização permite que as variáveis sejam
    //automaticamente sincronizadas
    //entre o cliente e o servidor
    //v1
    //[SyncVar(hook = nameof(OnColorChanged))] private Color playerColor = Color.white;
    //v2
    [SyncVar] private Color playerColor = Color.white;

    [SerializeField] private float moveSpeed = 5f;

    [Client]
    void Update()
    {
        if (!hasAuthority) { return; }

        //Guarda os valores que são primidos no
        //teclado em todas as direções
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        //Calcular o movimento com base na velocidade e no tempo
        Vector3 movement = new Vector3(horizontal, 0f, vertical) * 
            moveSpeed * Time.deltaTime;


        if(movement.magnitude > 0)
            CmdMove(movement);

        //Verificar a tecla que fio primida e trocar de cor
        if(Input.GetKeyDown(KeyCode.C))
            CmdChangedColor(new Color(Random.value, 
                Random.value, Random.value));
    }

    [Command]
    private void CmdMove(Vector3 movement)
    {
        //Validar a logica
        RpcMove(movement);

    }

    [Command]
    private void CmdChangedColor(Color newColor)
    {
        playerColor = newColor;
        RpcChangeColor(newColor);
    }

    [ClientRpc]
    private void RpcMove(Vector3 movement) => transform.Translate(movement);

    [ClientRpc]
    private void RpcChangeColor(Color newColor)
    {
        playerColor = newColor;
        OnColorChanged(playerColor, playerColor);
    }

    [Server]
    public override void OnStartServer()
    {
        //Sempre que inicia o servidor, uma cor é definida
        //ao jogador aleatóriamente
        playerColor = new Color(Random.value, Random.value, Random.value);
    }
    private void OnColorChanged(Color oldColor, Color newColor)
    {
        //Esta função vai ser chamada sempre que
        //a cor do jogador mudar
        GetComponent<Renderer>().material.color = newColor;
    }


}