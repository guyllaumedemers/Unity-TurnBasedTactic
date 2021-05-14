using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Panda;
using ScriptableObjects.Abilities;
using Random = UnityEngine.Random;

public class AIEnemy : Unit
{
    bool canTargetAGroup;
    public Vector3Int target;
    PlayerUnit targetPlayer;
    const int firstIndexSpell = 1;
    Ability choosedAbility;
    public bool wantToCompleteTurn;

    [Task]
    bool DoesWallBlockView()
    {
        return false;
    }

    [Task]
    public bool IsItMyTurn { get; set; }

    [Task]
    bool ChangeTurn()
    {
        IsItMyTurn = false;
        moveCompleted = false;
        return true;
    }
    [Task]
    bool canAttack()
    {
        bool canAttack = false;
        if (unitData.abilities.Length > 1)
        {
            if (unitData.abilities.Select(x => x is AoE||x is AoEHeal).Where(y => y == true).Any())
            {
                var Aoes = unitData.abilities.Where(x => x is AoE || x is AoEHeal).ToList();
                for (int i = 0; i < Aoes.Count; i++)
                {
                    foreach (var unitPos in UnitManager.Instance.unitDictionnary.Keys)
                    {
                        if (UnitManager.Instance.unitDictionnary[unitPos] is PlayerUnit)
                        {
                            bool istarget = true;
                            foreach (var unitPos2 in UnitManager.Instance.unitDictionnary.Keys)
                            {
                                if (UnitManager.Instance.unitDictionnary[unitPos2] is AIEnemy)
                                {

                                    if (Vector3Int.Distance(unitPos, unitPos2) <= (Aoes[i] as AoE)?.area
                                        || Vector3Int.Distance(unitPos, unitPos2) <= (Aoes[i] as AoEHeal)?.area)
                                    {
                                        istarget = false;
                                    }
                                }

                            }
                            if (istarget && unitData.stats.Ap > Aoes[i].apCost)
                            {
                                canAttack = true;
                                targetPlayer = UnitManager.Instance.unitDictionnary[unitPos] as PlayerUnit;
                                break;
                            }
                        }

                    }
                    if (canAttack)
                    {
                        choosedAbility = Aoes[i];
                        break;
                    }
                }
            }
        }
        return canAttack;
    }
    [Task]
    bool MorePointAction(bool ToMove)
    {
        bool actionPoint = true;
        //si le Unit cherche à déplacer
        if (ToMove)
        {
            if (GridManager.aStar.CheckIfEnoughActionPoint(this, target))
            {
                StartCoroutine(GridManager.aStar.GetPathCost(this, target));
                if (unitData.stats.Ap < AStar.movementCost + choosedAbility.apCost)
                {
                    actionPoint = false;
                }
            }
            else
            {
                actionPoint = false;
            }
           
            if (choosedAbility is Line)
            {
                foreach (var unit in UnitManager.Instance.unitDictionnary.Values)
                {
                    if (!unit.Equals(this)&&!unit.Equals(targetPlayer))
                    {
                        if (unit.positionGrid.x == targetPlayer.positionGrid.x && unit.positionGrid.x == target.x
                            &&(unit.positionGrid.y>Mathf.Min(targetPlayer.positionGrid.y,target.y) &&unit.positionGrid.y< Mathf.Max(targetPlayer.positionGrid.y, target.y)))
                        {
                            actionPoint = false;
                            break;
                        }

                        if (unit.positionGrid.y == targetPlayer.positionGrid.y && unit.positionGrid.y == target.y
                            && (unit.positionGrid.x > Mathf.Min(targetPlayer.positionGrid.x, target.x) && unit.positionGrid.x < Mathf.Max(targetPlayer.positionGrid.x, target.x)))
                        {
                            actionPoint = false;
                            break;
                        }
                    }
                    
                }
               
            }
        }
        //sinon il veut attaquer
        else
        {
            if (unitData.stats.Ap == 2 && Mathf.Approximately(Vector3Int.Distance(positionGrid, targetPlayer.positionGrid), 1))
            {
                choosedAbility = unitData.abilities[0];
            }
            if (unitData.stats.Ap < choosedAbility.apCost)
            {
                actionPoint = false;
            }
        }
        return actionPoint;
    }
    [Task]
    bool IsAnEnemyInRange()
    {
        bool AnEnemyInRange = false;
        Vector3Int oldTarget = target;
        foreach (var unitPos in UnitManager.Instance.unitDictionnary.Keys)
        {
            if (UnitManager.Instance.unitDictionnary[unitPos] is PlayerUnit)
            {
                if (GridManager.aStar.CheckIfEnoughActionPoint(this, unitPos + new Vector3Int(0, 1, 0), this)
                    || GridManager.aStar.CheckIfEnoughActionPoint(this, unitPos + new Vector3Int(1, 0, 0), this)
                    || GridManager.aStar.CheckIfEnoughActionPoint(this, unitPos + new Vector3Int(0, -1, 0), this)
                    || GridManager.aStar.CheckIfEnoughActionPoint(this, unitPos + new Vector3Int(-1, 0, 0), this))
                {
                    AnEnemyInRange = true;
                    break;
                }
            }
        }
        if (unitData.stats.Ap == 1)
        {
            AnEnemyInRange = false;
        }
        if (AnEnemyInRange)
        {
            target = oldTarget;
        }
        return AnEnemyInRange;
    }
    [Task]
    bool InRange()
    {
        //verifier s'il est dans le range avant de retourner vrai ou faux
        //pour l'instant prenons un range de 2
        //Debug.Log("calculation of the distance with the target to know if it's necessary to move or not");

        return Mathf.Approximately(Vector3Int.Distance(targetPlayer.positionGrid, positionGrid), 1);              // plus petit ou egal. On cherche a verifier si le voisin qui est a une distance de 1 case est InRange
    }


    [Task]
    bool Attack()
    {
        if ((choosedAbility is BaseAttack || choosedAbility is Taunt) && Mathf.Approximately(Vector3Int.Distance(positionGrid, targetPlayer.positionGrid), 1) || (!(choosedAbility is BaseAttack || choosedAbility is Taunt)))
        {
            if (choosedAbility is Buff)
            {
                choosedAbility.UseAbility(this, positionGrid);
            }
            else
            {
                choosedAbility.UseAbility(this, targetPlayer.positionGrid);
            }
  
        }
        else
        {
            if (unitData.abilities.Select(x => (x is Buff || x is AoE||x is AoEHeal)).Where(y => y == true).Any())
            {
                var buffAoe = unitData.abilities.Where(x => (x is Buff || x is AoE|| x is AoEHeal)).ToList();
                choosedAbility = buffAoe.ElementAt(Random.Range(0, buffAoe.Count()));
                if (choosedAbility is Buff)
                {
                    choosedAbility.UseAbility(this, positionGrid);
                }
                else
                {
                    choosedAbility.UseAbility(this, targetPlayer.positionGrid);
                }
 
            }
        }
        if (unitData.stats.Ap < unitData.stats.maxAp / 2)
        {
            int rdm = Random.Range(0, unitData.stats.Ap);
            if (rdm == 3 || rdm == 0)
            {
                wantToCompleteTurn = true;
            }
        }

        return true;
    }

    [Task]
    bool Move()
    {
        BeginMove(target);
        GridManager.aStar.GenerateMoves();
        GridManager.aStar.HandlePathNodeSelection(CompleteMove);
        EndMove();
        return true;
    }
    [Task]
    bool moveCompleted;
    [Task]
    bool WaitingToMove()
    {
        return true;
    }
    private void CompleteMove()
    {
        moveCompleted = true;
        if (unitData.stats.Ap<unitData.stats.maxAp/2)
        {
            int rdm = Random.Range(0, unitData.stats.Ap);
            if (rdm ==3 ||rdm==0)
            {
                wantToCompleteTurn = true;
            }
        }

    }
    /// <summary>
    /// retourner tous les groupes cibles possibles que l'IA peut attaquer
    /// </summary>
    /// <returns></returns>
    List<List<PlayerUnit>> GetAllGroups()
    {
        //if they are at a distance equal to 1 unit and my teamate is not in range
        List<PlayerUnit> players = new List<PlayerUnit>();
        PlayerUnit[] playerUnits = GameObject.FindObjectsOfType<PlayerUnit>();
        List<int> DistinctX = new List<int>();//liste des différentes positions en X des cibles
        List<int> DistinctY = new List<int>();//liste des différentes positions en Y des cibles

        foreach (PlayerUnit item in playerUnits)
        {
            players.Add(item);
            DistinctX.Add(item.positionGrid.x);
            DistinctY.Add(item.positionGrid.y);
        }
        List<List<PlayerUnit>> groups = new List<List<PlayerUnit>>();

        if (players.Count == 1)
        {
            List<PlayerUnit> group = new List<PlayerUnit>();
            group.Add(players[0]);
            groups.Add(group);
        }
        else if (players.Count > 1)
        {

            DistinctX = DistinctX.Distinct().ToList<int>();                 // why ToList<int> , its already a List<int>
            DistinctY = DistinctY.Distinct().ToList<int>();
            List<List<PlayerUnit>> sameX = new List<List<PlayerUnit>>();
            List<List<PlayerUnit>> sameY = new List<List<PlayerUnit>>();

            //Regrouper toutes les cibles qui sont sur la même ligne et celles qui sont sur la même colonne de la grille
            foreach (var x in DistinctX)
            {
                List<PlayerUnit> playersWithSameX = new List<PlayerUnit>();

                foreach (var player in players)
                {
                    if (player.positionGrid.x == x)
                    {
                        playersWithSameX.Add(player);
                    }
                }
                sameX.Add(playersWithSameX);
            }
            foreach (var y in DistinctY)
            {
                List<PlayerUnit> playersWithSameY = new List<PlayerUnit>();

                foreach (var player in players)
                {
                    if (player.positionGrid.y == y)
                    {
                        playersWithSameY.Add(player);
                    }
                }
                sameY.Add(playersWithSameY);
            }
            //retrouver tous les groupes possibles sachant qu'un groupe signifie que tous les Units du groupe sont 
            //à une distance de 1
            foreach (var players1 in sameX)
            {
                if (players1.Count == 1)
                {
                    if (sameY.Exists(x => x.Count == 1 && x[0].Equals(players1[0])))
                    {
                        groups.Add(players1);
                    }
                    else
                    {
                        foreach (var players2 in sameY)
                        {

                            if (players2.Contains(players1[0]) && players2.Count > 1)
                            {
                                List<PlayerUnit> players3 = players2.OrderBy(x => x.positionGrid.x).ToList();
                                int indice = players3.IndexOf(players1[0]);
                                if ((int)Vector3Int.Distance(players3[Mathf.Max(indice - 1, 0)].positionGrid, players1[0].positionGrid) != 1
                                    && (int)Vector3Int.Distance(players3[Mathf.Min(indice + 1, players3.Count - 1)].positionGrid, players1[0].positionGrid) != 1)
                                {
                                    groups.Add(players1);
                                }
                            }

                        }
                    }


                }
                else
                {

                    List<PlayerUnit> players3 = players1.OrderBy(x => x.positionGrid.y).ToList();
                    int startIndex = 0;
                    int count = 1;
                    List<List<PlayerUnit>> groupX = new List<List<PlayerUnit>>();
                    for (int i = 0; i < players3.Count - 1; i++)
                    {

                        if (Vector3Int.Distance(players3[i].positionGrid, players3[i + 1].positionGrid) == 1.0)
                        {
                            count++;

                        }
                        else
                        {
                            groupX.Add(players3.GetRange(startIndex, count));
                            startIndex = i + 1;
                            count = 1;

                        }

                        if (i == players3.Count - 2)
                        {
                            groupX.Add(players3.GetRange(startIndex, count));
                        }
                    }
                    foreach (var item in groupX)
                    {
                        if (item.Count > 1)
                        {
                            groups.Add(item);
                        }
                        else
                        {
                            if (sameY.Exists(x => (x.Count == 1 && x[0].Equals(item[0]))))
                            {
                                groups.Add(item);
                            }
                            else
                            {
                                foreach (var players2 in sameY)
                                {
                                    if (players2.Contains(item[0]) && players2.Count > 1)
                                    {
                                        List<PlayerUnit> players4 = players2.OrderBy(x => x.positionGrid.x).ToList();
                                        int indice = players4.IndexOf(item[0]);
                                        if ((int)Vector3Int.Distance(players4[Mathf.Max(indice - 1, 0)].positionGrid, item[0].positionGrid) != 1
                                            && (int)Vector3Int.Distance(players4[Mathf.Min(indice + 1, players4.Count - 1)].positionGrid, item[0].positionGrid) != 1)
                                        {
                                            groups.Add(item);
                                        }
                                    }
                                }

                            }
                        }
                    }

                }
            }

            foreach (var players1 in sameY)
            {
                if (players1.Count > 1)
                {

                    List<PlayerUnit> players3 = players1.OrderBy(x => x.positionGrid.x).ToList();
                    int startIndex = 0;
                    int count = 1;
                    List<List<PlayerUnit>> groupY = new List<List<PlayerUnit>>();
                    for (int i = 0; i < players3.Count - 1; i++)
                    {

                        if ((int)Vector3Int.Distance(players3[i].positionGrid, players3[i + 1].positionGrid) == 1)
                        {
                            count++;

                        }
                        else
                        {
                            groupY.Add(players3.GetRange(startIndex, count));
                            startIndex = i + 1;
                            count = 1;

                        }

                        if (i == players3.Count - 2)
                        {
                            groupY.Add(players3.GetRange(startIndex, count));
                        }
                    }
                    foreach (var item in groupY)
                    {
                        if (item.Count > 1)
                        {
                            groups.Add(item);
                        }

                    }

                }
            }


        }

        return groups;
    }
    /// <summary>
    /// Tache permettant à l'IA de trouver la position autour de la cible pour ce rendre
    /// </summary>
    [Task]
    void setTaget()
    {

        List<List<PlayerUnit>> groups = GetAllGroups();
        List<PlayerUnit> chooseList = ChooseGroupToAttack(groups);
        if (chooseList.Count > 1 && unitData.abilities.Select(x => x is Line )
            .Where(y => y == true).Any())
        {
            canTargetAGroup = true;
        }
        else
        {
            canTargetAGroup = false;
        }
        //si c'est un groupe, trouver la meilleure position autour du groupe
        target = positionGrid;
        if (canTargetAGroup)
        {

            bool targetChange = SetTargetAttackGroup(chooseList);

            if (!targetChange || !MorePointAction(true))
            {
                //choisir la cible la plus fragile et la plus proche
                while (!SetTargetPostionToSinglePlayer(groups) && groups.Count > 1)
                {
                    groups.Remove(chooseList);
                    chooseList = ChooseGroupToAttack(groups);
                    if (chooseList.Count > 1)
                    {
                        SetTargetAttackGroup(chooseList);

                    }
                    else
                    {
                        SetOptimalPositionArroundTarget(chooseList[0].positionGrid);
                    }
                }


            }
        }
        //sinon aller vers la seule cible
        else
        {
            SetTargetPostionToSinglePlayer(groups);
        }
        Task.current.Succeed();
    }
/// <summary>
/// choisir le groupe capable d'attaquer
/// </summary>
/// <param name="groups"></param>
/// <returns></returns>
    List<PlayerUnit> ChooseGroupToAttack(List<List<PlayerUnit>> groups)
    {

        groups = groups.OrderBy(x => Random.value).ToList();
        List<PlayerUnit> MaxLenghtGroup = groups[0];

        for (int i = 1; i < groups.Count; i++)
        {
            if (MaxLenghtGroup.Count < groups[i].Count)
            {
                Debug.Log(groups[i]);
                MaxLenghtGroup = groups[i];
            }
        }
        if (MaxLenghtGroup.Count > 1)
        {
            if (!unitData.abilities.Select(x => x is AoE|| x is AoEHeal).Where(y => y == true).Any())
            {
                List<PlayerUnit> isolatedTarget = new List<PlayerUnit>();
                foreach (var item in groups.Where(x => x.Count == 1).Distinct().ToList())
                {
                    isolatedTarget.Add(item[0]);
                }

                if (isolatedTarget.Count > 0)
                {
                    List<PlayerUnit> list = new List<PlayerUnit>();
                    list.Add(isolatedTarget[0]);
                    MaxLenghtGroup = list;
                }
            }

        }


        return MaxLenghtGroup;
    }

    /// <summary>
    /// trouver la position optimale autour de la cible
    /// </summary>
    /// <param name="playerPosition"></param>
    void SetOptimalPositionArroundTarget(Vector3Int playerPosition)
    {
        canTargetAGroup = false;
        choosedAbility = unitData.abilities[0];
        if (unitData.abilities.Select(x => x is Taunt).Where(y => y == true).Any())
        {
            if (Random.Range(0, 2) == 0)
            {
                var taunts = unitData.abilities.Where(x => (x is Taunt)).ToList();
                choosedAbility = taunts.ElementAt(Random.Range(0, taunts.Count()));
            }

        }
 
        List<Vector3Int> pos = new List<Vector3Int>();
        pos.Add(new Vector3Int(playerPosition.x, playerPosition.y + 1, 0));
        pos.Add(new Vector3Int(playerPosition.x, playerPosition.y - 1, 0));
        pos.Add(new Vector3Int(playerPosition.x + 1, playerPosition.y, 0));
        pos.Add(new Vector3Int(playerPosition.x - 1, playerPosition.y, 0));

        //Chercher la position la plus proche et dont l'IA a assez de point d'action pour se rendre et attaquer
        do
        {
            Vector3Int optimalPosition = pos[0];
            for (int i = 0; i < pos.Count; i++)
            {
                if (Vector3Int.Distance(optimalPosition, positionGrid) > Vector3Int.Distance(pos[i], positionGrid))
                {
                    optimalPosition = pos[i];
                }
            }
            target = optimalPosition;

            pos.Remove(optimalPosition);
        } while (pos.Count >= 1 && (!MorePointAction(true)) && !target.Equals(positionGrid));
        //si la position précédente trouvée n'était pas accessible, choisir une habilité Buff s'il y en a 
        if (!MorePointAction(true))
        {
            if (unitData.abilities.Select(x => x is Buff).Where(y => y == true).Any())
            {
                var lines = unitData.abilities.Where(x => (x is Buff)).ToList();
                choosedAbility = lines.ElementAt(Random.Range(0, lines.Count()));
            }

        }
    }

    /// <summary>
    /// choose target position when want to attack a single player
    /// </summary>
    bool SetTargetPostionToSinglePlayer(List<List<PlayerUnit>> groups)
    {
        
        List<PlayerUnit> isolatedTarget = new List<PlayerUnit>();
        //vérouiller toutes les cibles isolées
        foreach (var item in groups.Where(x => x.Count == 1).Distinct().ToList())
        {
            isolatedTarget.Add(item[0]);
        }
        //choisir la cible isolé la plus fragile
        if (isolatedTarget.Count > 1)
        {
            PlayerUnit targetWithMinHp = isolatedTarget.OrderBy(x => (x.unitData.stats.Hp)).ToList().First();
            targetPlayer = targetWithMinHp;
            SetOptimalPositionArroundTarget(targetWithMinHp.positionGrid);

            if (!MorePointAction(true))
            {
                PlayerUnit targetOptimal = isolatedTarget.OrderBy(x => Vector3Int.Distance(x.positionGrid, positionGrid)).ToList().First();
                target = targetOptimal.positionGrid;
                targetPlayer = targetOptimal;
                SetOptimalPositionArroundTarget(target);

            }
            return true;
        }
        else
        {

            List<PlayerUnit> playerUnits = new List<PlayerUnit>();

            foreach (var players in groups)
            {
                foreach (var player in players)
                {
                    playerUnits.Add(player);
                }
            }
            PlayerUnit targetWithMinHp = playerUnits[0];
            for (int i = 0; i < playerUnits.Count; i++)
            {
                if (targetWithMinHp.unitData.stats.Hp > playerUnits[i].unitData.stats.Hp)
                {
                    targetWithMinHp = playerUnits[i];
                }
            }
            targetPlayer = targetWithMinHp;
            SetOptimalPositionArroundTarget(targetWithMinHp.positionGrid);
            //Si la cible la plus fragile n'est pas accessible choisir la cible isolé la plus proche
            if (!MorePointAction(true))
            {
                PlayerUnit targetOptimal = playerUnits.OrderBy(x => Vector3Int.Distance(x.positionGrid, positionGrid)).ToList().First();
                target = targetOptimal.positionGrid;
                targetPlayer = targetOptimal;
                SetOptimalPositionArroundTarget(target);

            }
            return false;
        }

    }

    /// <summary>
    /// Choose target position when want to attack a group
    /// </summary>
    bool SetTargetAttackGroup(List<PlayerUnit> groupToAttack)
    {
        bool targetChange = false;
        canTargetAGroup = true;
        var lines = unitData.abilities.Where(x => x is Line).ToList();
        choosedAbility = lines.ElementAt(Random.Range(0, lines.Count()));

        if (groupToAttack[0].positionGrid.x == groupToAttack[1].positionGrid.x)
        {
            int maxY = groupToAttack.OrderBy(x => x.positionGrid.y).ToList().Last().positionGrid.y;
            int minY = groupToAttack.OrderBy(x => x.positionGrid.y).ToList().First().positionGrid.y;

            if (Vector3Int.Distance(positionGrid, new Vector3Int(groupToAttack[0].positionGrid.x, maxY + choosedAbility.range, 0)) <
                Vector3Int.Distance(positionGrid, new Vector3Int(groupToAttack[0].positionGrid.x, minY - choosedAbility.range, 0)))
            {
                //verifier si cette position est accessible
                Vector3Int v = new Vector3Int(groupToAttack[0].positionGrid.x, maxY + choosedAbility.range, 0);
                targetChange = true;
                target = v;
                targetPlayer = UnitManager.Instance.unitDictionnary[new Vector3Int(v.x, maxY, 0)] as PlayerUnit;
                if (!MorePointAction(true))
                {
                    Vector3Int pos = new Vector3Int(groupToAttack[0].positionGrid.x, minY - choosedAbility.range, 0);
                    target = pos;
                    targetPlayer = UnitManager.Instance.unitDictionnary[new Vector3Int(v.x, minY, 0)] as PlayerUnit;

                }
               
            }
            else
            {
                //verifier si cette position est accessible
                Vector3Int v = new Vector3Int(groupToAttack[0].positionGrid.x, minY - choosedAbility.range, 0);
                targetChange = true;
                target = v;
                targetPlayer = UnitManager.Instance.unitDictionnary[new Vector3Int(v.x, minY, 0)] as PlayerUnit;
                if (!MorePointAction(true))
                {
                    Vector3Int pos = new Vector3Int(groupToAttack[0].positionGrid.x, maxY + choosedAbility.range, 0);
                    target = pos;
                    targetPlayer = UnitManager.Instance.unitDictionnary[new Vector3Int(v.x, maxY, 0)] as PlayerUnit;

                }
                

            }


        }
        else
        {
            int maxX = groupToAttack.OrderBy(x => x.positionGrid.x).ToList().Last().positionGrid.x;
            int minX = groupToAttack.OrderBy(x => x.positionGrid.x).ToList().First().positionGrid.x;

            if (Vector3Int.Distance(positionGrid, new Vector3Int(maxX + choosedAbility.range, groupToAttack[0].positionGrid.y, 0)) <
                Vector3Int.Distance(positionGrid, new Vector3Int(minX - choosedAbility.range, groupToAttack[0].positionGrid.y, 0)))
            {
                //verifier si cette position est accessible
                Vector3Int v = new Vector3Int(maxX + choosedAbility.range, groupToAttack[0].positionGrid.y, 0);
                target = v;
                targetChange = true;
                targetPlayer = UnitManager.Instance.unitDictionnary[new Vector3Int(maxX, v.y, 0)] as PlayerUnit;

                if (!MorePointAction(true))
                {
                    Vector3Int pos = new Vector3Int(minX - choosedAbility.range, groupToAttack[0].positionGrid.y, 0);
                    target = pos;
                    targetPlayer = UnitManager.Instance.unitDictionnary[new Vector3Int(minX, v.y, 0)] as PlayerUnit;

                }
                
            }
            else
            {
                //verifier si cette position est accessible
                Vector3Int v = new Vector3Int(minX - choosedAbility.range, groupToAttack[0].positionGrid.y, 0);
                target = v;
                targetChange = true;
                targetPlayer = UnitManager.Instance.unitDictionnary[new Vector3Int(minX, v.y, 0)] as PlayerUnit;
                if (!MorePointAction(true))
                {
                    Vector3Int pos = new Vector3Int(maxX + choosedAbility.range, groupToAttack[0].positionGrid.y, 0);
                    target = pos;
                    targetPlayer = UnitManager.Instance.unitDictionnary[new Vector3Int(maxX, v.y, 0)] as PlayerUnit;
                }
     

            }
        }
        return targetChange;

    }

    /// <summary>
    /// Add buff modifications
    /// </summary>
    /// <param name="ability"></param>
    public override void UseBuff(Ability ability)
    {
        ModStats(((Buff)ability).mods.stats);
    }
}
