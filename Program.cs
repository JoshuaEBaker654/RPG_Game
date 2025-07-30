namespace RPG_Game
{
// PLAYER CLASS 

	using System;
	using System.Collections;
	using System.Security.Cryptography;
	using System.Runtime.InteropServices;
	public partial class Player
	{

		private int health;
		public string Name { get; }

		public int Health
		{
			get { return health; }
			set { health = value; }
		}

		public int Attackstrength { get; }

		public int Mana
		{
			get { return mana; }
			set { mana = value; }
		}

		public int Speed { get; }
		public int MaxHealth { get; private set; }

		public Item[] inventory;

		public Item[] Inventory
		{
			get { return inventory; }
		}

		public string spellbook { get; }

// arrays //
		public Spell[] Spellbook = new Spell[3];

		private int mana = 100;

		// constructor //
		public Player(string combatantName, int startingHealth, int baseAttack, int combatSpeed)
		{
			Name = combatantName;
			Health = startingHealth;
			Attackstrength = baseAttack;
			Speed = combatSpeed;
			MaxHealth = startingHealth;
			inventory = new Item[5];

		}

		public Player()
		{
			Name = "Unknown";
			Health = 0;
			Attackstrength = 0;
		}

		public Enemy Attack(Enemy enemy)
		{
			Console.WriteLine($"Player: {Name}, Health: {Health}, Attackstrength: {Attackstrength}");
			int totalDamage = Attackstrength;
			foreach (var item in inventory)
			{
				if (item != null)
				{
					totalDamage += item.Power;
				}
			}

			enemy.Health -= totalDamage;
			return enemy;
		}

		public bool AddItem(Item item)
		{
			for (int i = 0; i < inventory.Length; i++)
			{
				if (inventory[i] == null)
				{
					inventory[i] = item;
					return true;
				}
			}

			return false;
		}

		public bool LearnSpell(Spell spell)
		{
			for (int i = 0; i < Spellbook.Length; i++)
			{
				if (Spellbook[i] == null)
				{
					Spellbook[i] = spell;
					return true;
				}
			}

			return false;
		}

		public bool HasSpell(Spell spell)
		{
			foreach (var s in Spellbook)
			{
				if (s != null && s.Equals(spell))
				{
					return true;
				}
			}

			return false;
		}

		public Enemy CastSpell(Enemy enemy, Spell spell)
		{

			if (HasSpell(spell) && Mana >= spell.ManaCost)
			{
				enemy.Health -= spell.Power;
				mana -= spell.ManaCost;
			}

			return enemy;
		}

		public void UsePotion(Item potion)
		{
			for (int i = 0; i < inventory.Length; i++)
			{
				// Check if the potion is in inventory
				if (inventory[i] == potion)
				{
					// Apply HealthPotion effect
					if (potion.Name == "HealthPotion")
					{
						Health += potion.Power;
						if (Health > MaxHealth) Health = MaxHealth; // Cap health
					}
					// Apply ManaPotion effect
					else if (potion.Name == "ManaPotion")
					{
						Mana += potion.Power;
						if (Mana > 100) Mana = 100; // Cap mana
					}

					// Remove potion from inventory
					inventory[i] = null;
					break; // Exit the loop after using the potion
				}
			}
		}

		public bool RemoveItem(Item item)
		{
			for (int i = 0; i < inventory.Length; i++)
			{
				if (inventory[i] == item)
				{
					inventory[i] = null;
					return true;
				}
			}

			return false;
		}

		public override string ToString()
		{
			return $"Name:{Name}\nHP:{Health}\nSpeed:{Speed}\nMana:{Mana}\nAttack Strength:{Attackstrength}.";
		}
	}





// WAVE MANAGER

	public class WaveManager
	{
		private Stack<Enemy[]> _remainingWaves;
		//public WaveGenerator WaveGenerator { get; set; }

		public WaveManager(int totalWaves)
		{
			_remainingWaves = new Stack<Enemy[]>();
			for (int i = totalWaves; i >= 1; i--)
			{
				Enemy[] wave = WaveGenerator.GenerateWave(i);
				_remainingWaves.Push(wave);
			}
		}

		// HASNEXTWAVE

		public bool HasNextWave()
		{
			if (_remainingWaves.Count > 0)
			{
				return true;
			}

			return false;
		}

		// GETNEXTWAVE  
		public Enemy[] GetNextWave()
		{
			if (_remainingWaves.Count == 0)
			{
				return null;
			}

			return _remainingWaves.Pop();
		}

		public Item GenerateDrop(double dropRate)
		{
			// Use the provided DropGenerator class:
			if (DropGenerator.ShouldGenerateDrop(dropRate))
			{
				return DropGenerator.GenerateRandomPotion();
			}

			return null;
		}
	}


	// WAVE GEN 
	public static class WaveGenerator
	{
		private static Random _random = new Random();

		public static Enemy[] GenerateWave(int waveNumber, int baseHealth = 50, int baseStrength = 10)
		{
			// Generate 2-4 enemies per wave
			int enemyCount = _random.Next(2, 5);
			Enemy[] enemies = new Enemy[enemyCount];

			// Increase difficulty with wave number
			int healthScale = baseHealth + (waveNumber * 10);
			int strengthScale = baseStrength + (waveNumber * 2);

			for (int i = 0; i < enemyCount; i++)
			{
				enemies[i] = new Enemy(
					$"Wave {waveNumber} Enemy {i + 1}",
					healthScale,
					strengthScale,
					_random.Next(20, 41) // Random speed 20-40
				);
			}

			return enemies;
		}
	}


// DROP GEN 
	public static class DropGenerator
	{
		private static Random _random = new Random();

		public static bool ShouldGenerateDrop(double dropRate)
		{
			if (dropRate < 0.0 || dropRate > 1.0)
				throw new ArgumentException("Drop rate must be between 0.0 and 1.0");

			return _random.NextDouble() < dropRate;
		}

		public static Item GenerateRandomPotion()
		{
			if (_random.Next(2) == 0)
				return new Item("HealthPotion", _random.Next(15, 51), true);
			else
				return new Item("ManaPotion", _random.Next(15, 51), true);
		}
	}

// GAME MAN

	public class GameManager
	{
		private WaveManager _waveManager;
		private CombatManager _combatManager;
		private Player _player;
		private double _dropRate;

		public GameManager(int totalWaves, double dropRate, Player aplayer)
		{
			_player = new Player();
			_waveManager = new WaveManager(totalWaves);
			_dropRate = dropRate;
		}

		// TODO: Fill in all the areas with comments in this method
		public bool PlayGame()
		{
			Console.WriteLine("**************************************");
			Console.WriteLine("*        Fighting Game Start!        *");
			Console.WriteLine("**************************************\n");
			Console.WriteLine(_player);

			// Create a boolean that tracks if the player is alive.
			bool isAlive = true;
			// While the wave manager still has waves remaining:
			while (_waveManager.HasNextWave())
			{
				Console.WriteLine("Press any key to begin the next round...");
				Console.ReadKey();
				Console.WriteLine("\n===============================================\n");
				Console.WriteLine("ROUND BEGIN!");
				// Get and process the next wave, storing the result in the boolean value

				Enemy[] currentWave = _waveManager.GetNextWave();
				isAlive = ProcessWave(currentWave);

				Console.WriteLine("ROUND OVER!");
				Console.WriteLine("\n===============================================\n");
				// If the player is not alive:
				if (!isAlive)
				{
					Console.WriteLine("======================================");
					Console.WriteLine("=              YOU LOSE              =");
					Console.WriteLine("======================================");
					return false;
				}
				// Otherwise:
				else
				{
					Console.WriteLine(_player);
					int choice = int.Parse(Console.ReadLine());

					// While this int is greater than the player's inventory array length or less than -1
					while (choice < -1 || choice > _player.inventory.Length)
					{
						// Ask the user to pick the index of a potion to use, or -1 to not use any (validate this input)
						Console.WriteLine("Enter the index of a portion to use (0 to {0}).", choice);
						string input = Console.ReadLine();
					}

					// If this int is not equal to -1:
					if (choice != -1)
					{
						//Use the potion in the player's inventory that the user gave.
						Console.WriteLine($"Using {_player.Inventory[choice]}");
					}
					// Otherwise, do nothing.
					else
						Console.WriteLine("No potions used.");
				}
			}

			Console.WriteLine("======================================");
			Console.WriteLine("                YOU WIN               ");
			Console.WriteLine("======================================");
			// If all waves completed, return true.
			return isAlive;
		}

		private bool ProcessWave(Enemy[] enemies)
		{
			// TODO: Wave combat loop (READ VERY CAREFULLY!!!!)
			// 1. Initialize the game manager's CombatManager with a new CombatManager with current player and enemies.
			_combatManager = new CombatManager(_player, enemies);
			// 2. Order all the enemies in game manager's CombatManager.
			_combatManager.OrderAllEnemies();
			// 3. While there is at least one enemy alive:
			//    - Display number of enemies left.
			//    - Process turns using speed order:
			//        - If the current enemy in combat manager is alive: 
			// 			  - and if this enemy's speed is greater than player's OR player has already attacked: 
			//			      - current enemy in combat manager attacks player in combat manager.
			//			      - If player died after this attack, return false;
			//        	  - Otherwise:
			//			      - Ask user to input 1 to attack or 2 to cast a spell (validate this input)
			//			      - If user input 1, player in combat manager attacks current enemy in combat manager
			//			      - Otherwise:
			//				      - Ask the user to input the index of the spell they want to use (validate this input)
			//					  - Player in combat manager cast's spell of that index on current enemy in combat manager
			//			  	  - If current enemy died after this attack, call HandleEnemyDeath
			//    - After all turns in a round, store the updated player in combat manager in the game manager's player
			// 4. Return true if wave completed
			while (EnemiesRemaining(enemies) > 0)
			{
				Console.WriteLine($"Number of remaining enemies: {EnemiesRemaining(enemies)}");

				// Get the battle order (sorted by speed or other criteria)
				foreach (var enemy in _combatManager.ActiveEnemies)
				{
					if (enemy.Health > 0)
					{
						// If the enemy's speed is greater than the player's OR the player has already acted:
						if (enemy.Speed > _player.Speed)
						{
							// Enemy attacks the player
							enemy.Attack(_player);

							// If the player's health drops to 0, return false (wave failed)
							if (_player.Health <= 0)
							{
								Console.WriteLine("Player has died. Wave failed.");
								return false;
							}
						}

						else
						{
							Console.WriteLine("Pick your action:\n1: Attack\n2: Cast Spell");
							int action;
							int.TryParse(Console.ReadLine(), out action);

							if (action == 1)
							{
								Console.WriteLine($"Attacking {enemy.Name}!");
								_player.Attack(enemy);
							}
							else if (action == 2)
							{
								Console.WriteLine("Enter spell index:");
								int spellIndex;
								int.TryParse(Console.ReadLine(), out spellIndex);

								if (spellIndex >= 0 && spellIndex < _player.Spellbook.Length)
								{
									_player.CastSpell(enemy, _player.Spellbook[spellIndex]);
								}
								else
								{
									Console.WriteLine("The spell index is invalid.");
								}
							}

							if (enemy.Health <= 0)
							{
								HandleEnemyDeath(enemy);
							}
						}
					}
				}

				// Update the game manager's player with the updated player state
			}

			_player = _combatManager.ActivePlayer;
			return true;
		}
		// 4. If the loop completes, return true (wave completed)

		private int EnemiesRemaining(Enemy[] enemies)
		{
			// TODO: Enemy status check
			int count = 0;
			foreach (var enemy in enemies)
			{
				if (enemy.Health > 0)
				{
					count++;
				}
			}

			return count;
		}

		private void HandleEnemyDeath(Enemy enemy)
		{
			// TODO: Generate and handle drops
			Console.WriteLine($"{enemy.Name} has been defeated!");
			// 1. Use WaveManager to generate drop
			Item drop = _waveManager.GenerateDrop(2);
			// 2. If item dropped, try to add to player's inventory
			if (drop != null)
			{
				Console.WriteLine($"You found a {drop.Name}!");
			}

			bool added = _player.AddItem(drop);
			if (added)
			{
				Console.WriteLine($"{drop.Name}");
			}
		}
	}

// ENEMY 

	public class Enemy
	{

		private int health;
		public string Name { get; }

		public int Health
		{
			get { return health; }
			set { health = value; }
		}

		public int Attackstrength { get; }
		public int Speed { get; }
		public Item Inventory { get; }
		public Player Player { get; set; }

		public Enemy(string combatantName, int startingHealth, int baseAttack, int combatSpeed)
		{
			Name = combatantName;
			Health = startingHealth;
			Attackstrength = baseAttack;
			Speed = combatSpeed;
		}

		public Enemy()
		{
			Name = "Unknown";
			Health = 0;
			Attackstrength = 0;
		}

		public override string ToString()
		{
			return $"Enemy Name: {Name} Health:{Health} Attackstrength:{Attackstrength}";
		}

		// Enemy attack method //
		public Player Attack(Player pl)
		{
			Console.WriteLine($"Player: {Name}, Health: {Health}, Attackstrength: {Attackstrength}");
			Player.Health -= Attackstrength;
			return Player;
		}
	}

// SPELL 

	


	public class Spell
	{
		public string Name { get; }
		public int Power { get; }
		public int ManaCost { get; }

		public Spell(string name, int power, int manaCost)
		{
			Name = name;
			Power = power;
			ManaCost = manaCost;
		}

		public override bool Equals(object obj)
		{
			if (obj is Spell other)
			{
				return Name == other.Name && Power == other.Power && ManaCost == other.ManaCost;
			}

			return false;
		}


		public override string ToString()
		{
			return $"({Name}, {Power}P, {ManaCost}M)";
		}
	}


// ITEM 


	public class Item
	{
		public string Name { get; }
		public int Power { get; }
		public bool isPotion { get; }

		public Item(string name, int power, bool aPotion)
		{
			Name = name;
			Power = power;
			isPotion = aPotion;
		}

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
				return false;

			Item other = (Item)obj;
			return Name == other.Name && Power == other.Power && isPotion == other.isPotion;
		}


		public override string ToString()
		{
			return $"Item Name: {Name}Power:{Power}P";
		}

		public object this[int choice]
		{
			get { throw new NotImplementedException(); }
		}
	}

// COMBAT MAN


	class CombatManager
	{
		public Enemy[] ActiveEnemies { get; set; }
		public Player ActivePlayer { get; set; }


// Constructor clearly names what it's storing
		public CombatManager(Player activePlayer, Enemy[] startingEnemies)
		{
			ActiveEnemies = startingEnemies;
			ActivePlayer = activePlayer;
		}


		// Notice how the tuple names make the return value self-documenting
		public (Enemy faster, Enemy slower) OrderBySpeed(Enemy first, Enemy second)
		{
			if (first.Speed > second.Speed)
			{
				return (first, second);
			}

			return (second, first);
		}
// Orders two enemies by their speed (higher speed first)
// Example usage in Program.cs:
// var (fasterEnemy, slowerEnemy) = OrderBySpeed(enemyA, enemyB);

		public void OrderAllEnemies()
		{
			for (int i = 0; i < ActiveEnemies.Length; i++)
			{
				int maxIndex = i;
				for (int j = i + 1; j < ActiveEnemies.Length; j++)
				{
					var (faster, slower) = OrderBySpeed(ActiveEnemies[i], ActiveEnemies[j]);
					if (faster == ActiveEnemies[j])
					{
						maxIndex = j;
					}
				}

				if (maxIndex != i)
				{
					var temp = ActiveEnemies[i];
					ActiveEnemies[i] = ActiveEnemies[maxIndex];
					ActiveEnemies[maxIndex] = temp;
				}
			}
		}
// Sorts ActiveEnemies array from fastest to slowest
// Uses OrderBySpeed to compare enemies
// Implementation should use selection sort:
//   1. Find the fastest remaining enemy
//   2. Place it in the correct position
//   3. Repeat for each position

		public string GetBattleOrder() //ToString
		{
			OrderAllEnemies();
			string battleOrder = "Battle Order: \n";

			for (int i = 0; i < ActiveEnemies.Length; i++)
			{
				battleOrder += $"{i + 1}. {ActiveEnemies[i].Name} (Speed: {ActiveEnemies[i].Speed})\n";
			}

			battleOrder += $"{ActiveEnemies.Length + 1}. {ActivePlayer.Name} (Speed: {ActivePlayer.Speed})";

			return battleOrder;
		}
// Returns a clear description of who acts in what order
// Example output in Program.cs:
// "Battle Order:
//  1. Swift Rogue (Speed: 40)
//  2. Hero (Speed: 35)
//  3. Slow Ogre (Speed: 20)"

		
		static void Main(string[] args)
		{ 
			// Create a player
			Player player = new Player("Hero", 100, 20, 30);

			// Add some items to player's inventory
			player.AddItem(new Item("HealthPotion", 25, true));
			player.AddItem(new Item("ManaPotion", 30, true));

			// Teach the player some spells
			player.LearnSpell(new Spell("Fireball", 40, 20));
			player.LearnSpell(new Spell("Lightning", 35, 15));

			// Set up the game manager (e.g. 3 waves, 50% drop rate)
			GameManager game = new GameManager(totalWaves: 3, dropRate: 0.5, aplayer: player);

			// Start the game
			bool result = game.PlayGame();

			// End game message
			Console.WriteLine(result ? "Game Complete - Victory!" : "Game Over - Defeat");
			Console.ReadKey();
				
		}
	}
}
