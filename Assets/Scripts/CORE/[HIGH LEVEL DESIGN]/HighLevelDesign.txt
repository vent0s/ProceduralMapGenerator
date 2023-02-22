/*
 * [ABOUT MVC]
 * 
 * Since this gonna be a complecate strategy game(like paradox and civilization), we might have sohpiticated data sturcture and data relationship.
 * Even in very early stage of development, we can see the complexity that reveling up in our tile related system,
 * In previous draft(the first draft) of this game, I was trying to implement linked list as data storage method, which turns out into a total mess.
 * Then, I tried using dictionaries to store and query data, it some how works, but extreamly tedious and vulnerble in implementation.
 * Therefore, Im going to implement MVC architechture in current draft.
 * 
 *  The MVC architecture involving three major parts:
 *      Model
 *      View
 *      Controller
 *  In this game, for example, the mapsystem will implement MVC as:
 *      Model - Tile
 *      View - map displayer(this can be the map in game as we will twick in game behaviour in data handler as well)
 *      Controller - data handler
 *  We pass and aquire data in data handler, it process data from player or system, to sql table; or from sql table to player or system.
 *  Yet in later design, we mighgt need to seperate handler for each part of functions to prevent over coupling
 */