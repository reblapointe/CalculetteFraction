using System;

namespace CalculetteFraction
{
    class Program
    {
        /// <summary>
        /// Précision pour les fraction. On dira que deux nombres sont égaux si leur différence 
        /// est inférieure à la précision.
        /// </summary>
        const double PRECISION = 0.000_000_1;

        /// <summary>
        /// Calcule le plus grand commun diviseur (PGCD) entre deux entiers
        /// </summary>
        /// <remarks>
        /// Il applique l'algorithme d'Euclide
        /// </remarks>
        /// 
        /// <example> (12,66) --> 6   </example>
        /// <example> (5, 7) --> 1    </example>
        /// <example> (45, 90) --> 45 </example>
        /// 
        /// <param name="a">Un premier entier</param>
        /// <param name="b">Un deuxieme entier</param>
        /// <returns>Le plus grand commun diviseur comment à a et b</returns>
        static int PlusGrandCommunDiviseur(int a, int b)
        {
            if (b == 0)
                return a;
            return PlusGrandCommunDiviseur(b, a % b);
        }

        /// <summary>
        /// Réduit à fraction à sa plus simple expression. Les arguments
        /// sont modifiés.
        /// </summary>
        /// <example> (12, 24) --> (1, 2)  </example>
        /// <example> (75, 100) --> (3, 4) </example>
        ///   
        /// <param name="numerateur">Le numérateur de la fraction</param>
        /// <param name="denominateur">Le dénominateur de la fraction</param>
        static void ReduireFraction(ref int numerateur, ref int denominateur)
        {
            int pgcd = PlusGrandCommunDiviseur(numerateur, denominateur);
            numerateur /= pgcd;
            denominateur /= pgcd;
            if (denominateur < 0)
            {
                numerateur *= -1;
                denominateur *= -1;
            }
        }

        /// <summary>
        /// Détermine si deux doubles sont à peu près égaux selon une précision
        /// </summary>
        /// 
        /// <example> (1, 1.0001, 0.000000001) --> Vrai </example>
        /// <example> (0.25, 0.254, 0.01) --> Faux      </example>
        /// 
        /// <param name="a">premier nombre</param>
        /// <param name="b">deuxième nombre</param>
        /// <param name="precision">précision permise</param>
        /// <returns>Vrai si les deux nombres sont très proches (plus moins la précision), 
        /// faux sinon</returns>
        static bool APeuPres(double a, double b, double precision = PRECISION)
        {
            return a > b - precision / 2 && a < b + precision / 2;
        }

        /// <summary>
        /// Bâtit une fraction à peu près équivalente à un nombre à virgule. Le dénominateur ne va pas
        /// dépasser le max fourni, et la fraction va être approximativement égale au nombre plus ou
        /// moins la précision.  
        /// </summary>
        /// <remarks>
        /// Il applique l'algorithme de 
        /// <see href="https://www.johndcook.com/blog/2010/10/20/best-rational-approximation/">Farey</see>
        /// </remarks>
        /// 
        /// <example>(0.5) --> (1, 2)</example>
        /// <example>(0.3333333333) --> (1, 3)</example>
        /// <example>(1.010101010101) --> (100, 99)</example>
        /// 
        /// <param name="x">Un nombre à virgule</param>
        /// <param name="numerateur">Le numérateur de la fraction équivalente</param>
        /// <param name="denominateur">Le dénominateur de la fraction équivalente</param>
        /// <param name="denominateurMax">La valeur maximum du dénominateur</param>
        /// <param name="precision">La précision demandée</param>
        static void Farey(double x, out int numerateur, out int denominateur,
            int denominateurMax = 1_000_000_000, double precision = PRECISION)
        {
            int signe = 1;
            int partieEntiere = (int)x;
            if (APeuPres(x, partieEntiere, precision))
            {
                numerateur = partieEntiere;
                denominateur = 1;
                return;
            }
            x -= partieEntiere;
            if (x < 0)
            {
                signe = -1;
                x = -x;
            }

            int a = 0, b = 1, c = 1, d = 1;

            while (b <= denominateurMax && d <= denominateurMax)
            {
                double mediant = (double)(a + c) / (b + d);
                if (APeuPres(x, mediant, precision))
                {
                    if (b + d <= denominateurMax)
                    {
                        denominateur = b + d;
                        numerateur = signe * (partieEntiere * denominateur + a + c);
                        return;
                    }
                    else if (d > b)
                    {
                        denominateur = d;
                        numerateur = signe * (partieEntiere * denominateur + c);
                        return;
                    }
                    else
                    {
                        denominateur = b;
                        numerateur = signe * (partieEntiere * denominateur + a);
                        return;
                    }
                }
                else if (x > mediant)
                {
                    a += c;
                    b += d;
                }
                else
                {
                    c += a;
                    d += b;
                }
            }

            if (b > denominateurMax)
            {
                denominateur = d;
                numerateur = signe * (partieEntiere * denominateur + c);
            }
            else
            {
                denominateur = b;
                numerateur = signe * (partieEntiere * denominateur + a);
            }
        }

        /// <summary>
        /// Demande un nombre à l'utilisateur et lui donne la fraction équivalente
        /// </summary>
        static void TrouverFraction()
        {
            Console.WriteLine("Entrez un nombre à virgule");
            if (double.TryParse(Console.ReadLine(), out double n))
            {
                Farey(n, numerateur: out int a, denominateur: out int b, denominateurMax:int.MaxValue);
                Console.WriteLine($"{n} = {FractionVersString(a, b)}");
            }
            else
            {
                Console.WriteLine("Ceci n'est pas un nombre valide");
            }
        }

        /// <summary>
        /// Transforme une chaîne de caractère en la fraction correspondante.
        /// Si la fractions snt invalides, elle est initialisée à 0/1
        /// </summary>
        /// <example>"2/3" --> 2, 3. Vrai</example>
        /// <example>"2" --> 2, 1. Vrai</example>
        /// <example>"patate" --> 0, 1. Faux</example>
        /// <param name="entree">La chaîne de caractère qui est supposée contenir une fraction</param>
        /// <param name="numerateur">Le numérateur de la fraction</param>
        /// <param name="denominateur">Le dénominateur de la fraction</param>
        /// <returns>Vrai si la chaîne contient une fraction correcte, faux sinon</returns>
        static bool StringVersFraction(string entree, out int numerateur, out int denominateur)
        {
            numerateur = 0;
            denominateur = 1;
            int indiceSeparateur = entree.IndexOf('/');
            if (indiceSeparateur < 0)
                return int.TryParse(entree, out numerateur);
            bool bonNum = int.TryParse(entree[0..indiceSeparateur], out numerateur);
            bool bonDen = int.TryParse(entree[(indiceSeparateur + 1)..], out denominateur) && denominateur != 0;
            if (denominateur == 0)
                denominateur = 1;
            return bonNum && bonDen;
        }

        /// <summary>
        /// Transforme un fraction en une chaîne de caractère correspondante
        /// </summary>
        /// <example>(2, 3) --> "2/3"</example>
        /// <example>(2, 1) --> "2"</example>
        /// <param name="numerateur"></param>
        /// <param name="denominateur"></param>
        /// <returns>La fraction sous forme de string</returns>
        static string FractionVersString(int numerateur, int denominateur = 1)
        {
            string fraction = numerateur.ToString();
            if (denominateur != 1)
                fraction += "/" + denominateur;
            return fraction;
        }

        /// <summary>
        /// Demande à l'utilisateur d'entrer deux fractions. Si les fractions snt invalides, elles sont
        /// initialisées à 0/1.
        /// </summary>
        /// <param name="n1">Numérateur de la première fraction</param>
        /// <param name="d1">Dénominateur de la première fraction</param>
        /// <param name="n2">Numérateur de la deuxième fraction</param>
        /// <param name="d2">Dénominateur de la deuxième fraction</param>
        /// <returns>Vrai si les deux fractions entrées sont correctes, faux sinon.</returns>
        static bool LireDeuxFractions(out int n1, out int d1, out int n2, out int d2)
        {
            n2 = 0;
            d2 = 1;
            Console.WriteLine("Entrez la première fraction (a/b) où b != 0");
            string f1 = Console.ReadLine();
            if (StringVersFraction(f1, out n1, out d1))
            {
                Console.WriteLine("Entrez la deuxième fraction (a/b) où b != 0");
                string f2 = Console.ReadLine();
                if (StringVersFraction(f2, out n2, out d2))
                    return true;
            }
            Console.WriteLine("Fraction(s) invalide(s)!");
            return false;
        }

        /// <summary>
        /// Calcule la somme de deux fractionss
        /// </summary>
        /// <param name="n1">Numérateur de la première fraction</param>
        /// <param name="d1">Dénominateur de la première fraction</param>
        /// <param name="n2">Numérateur de la deuxiême fraction</param>
        /// <param name="d2">Dénominateur de la deuxiême fraction</param>
        /// <param name="ns">Numérateur de la fraction somme</param>
        /// <param name="ds">Dénominateur de la fraction somme</param>
        static void AdditionnerFractions(int n1, int d1, int n2, int d2, out int ns, out int ds)
        {
            ns = n1 * d2 + n2 * d1;
            ds = d1 * d2;
            ReduireFraction(numerateur: ref ns, denominateur: ref ds);
        }

        /// <summary>
        /// Demande à l'utilisateur d'entrer deux fractions et affiche la somme
        /// </summary>
        static void AdditionnerFractions()
        {
            Console.WriteLine("Addition de deux fractions.");
            if (LireDeuxFractions(out int n1, out int d1, out int n2, out int d2))
            {
                AdditionnerFractions(n1, d1, n2, d2, out int n, out int d);
                Console.WriteLine(
                    $"{FractionVersString(denominateur: d1, numerateur: n1)} + " +
                    $"{FractionVersString(numerateur: n2, denominateur: d2)} = " +
                    $"{FractionVersString(numerateur: n, denominateur: d)}");
            }
        }

        /// <summary>
        /// Calcule le produit de deux fractionss
        /// </summary>
        /// <param name="n1">Numérateur de la première fraction</param>
        /// <param name="d1">Dénominateur de la première fraction</param>
        /// <param name="n2">Numérateur de la deuxiême fraction</param>
        /// <param name="d2">Dénominateur de la deuxiême fraction</param>
        /// <param name="ns">Numérateur de la fraction produit</param>
        /// <param name="ds">Dénominateur de la fraction produit</param>
        static void MultiplierFractions(int n1, int d1, int n2, int d2, out int ns, out int ds)
        {
            ns = n1 * n2;
            ds = d1 * d2;
            ReduireFraction(ref ns, ref ds);
        }

        /// <summary>
        /// Demande à l'utilisateur d'entrer deux fractions et affiche le produit
        /// </summary>
        static void MultiplierFractions()
        {
            Console.WriteLine("Multiplication de deux fractions.");
            if (LireDeuxFractions(out int n1, out int d1, out int n2, out int d2))
            {
                MultiplierFractions(n1, d1, n2, d2, out int n, out int d);
                Console.WriteLine(
                    $"{FractionVersString(denominateur: d1, numerateur: n1)} * " +
                    $"{FractionVersString(numerateur: n2, denominateur: d2)} = " +
                    $"{FractionVersString(numerateur: n, denominateur: d)}");
            }
        }

        /// <summary>
        /// Offre une calcuatrice de fractions à l'utilisateur
        /// </summary>
        static void Main()
        {
            bool continuer = true;
            do
            {
                Console.WriteLine("***********************************************************");
                Console.WriteLine("Bienvenue dans la calculette de fractions");
                Console.WriteLine("A) Trouver la fraction équivalente à un nombre à virgule");
                Console.WriteLine("B) Additionner deux fractions");
                Console.WriteLine("C) Multiplier deux fractions");
                Console.WriteLine("Q) Quitter");
                Console.WriteLine("***********************************************************");
                string choix = Console.ReadLine();
                Console.Clear();
                switch (choix)
                {
                    case "A": case "a": case "1": TrouverFraction(); break;
                    case "B": case "b": case "2": AdditionnerFractions(); break;
                    case "C": case "c": case "3": MultiplierFractions(); break;
                    case "Q": case "q": case "4": continuer = false; break;
                    default: Console.WriteLine("Choix invalide."); break;
                }
                if (continuer)
                {
                    Console.WriteLine("Appuyez sur une touche pour continuer.");
                    Console.ReadKey();
                }
                Console.Clear();
            } while (continuer);
            Console.WriteLine("Merci d'avoir utilisé la calculette de fractions");
        }
    }
}
