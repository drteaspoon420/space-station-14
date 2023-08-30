# True Magic
Magic system that attempts to create system that is filled with discovery and experimentation. Idea is to use all availble game systems, generate new spells every game.

## Acquisition
Character may acquire 'true magic' by consuming chemical/reagent 'Unstable Bluespace Liquid' or 'Bluespace Liquid'. 'Unstable Bluespace Liquid' can be grinded from 'Bluespace Crystals' and then processed by mixing it with milk and tea to create stable 'Bluespace Liquid'.
Character can use 'True Magic' as long as they have either in their system. Unstable version causes minor brain damage every time character uses magic.

## Core Systems
True Magic is divided into X smaller sub systems. Speech that triggers magical effects. 'Delivery' methods for the magic that picks the effected entities and location of the magic. Magical effects that are split into different systems they take advantage of. Also every spell may chain into secondary effects and 'delivery methods'.

### Speech to Magic
While the user has 'True Magic' they cannot speak without casting magic. Every speech is followed by short mute to prevent spaming. When character speaks, every legible letter is taken and matched with a look up table. Most special characters are ignored. All character specific speech changes are included. This means pirates have troublew using same spells than non-pirates. Commas, periods, exclamation marks and question marks are used to split speech elements into more complicated spells.

The look up table for characters has array of 32, unsigned 8bit integer values that are randomized at round start. When looking up with a character, the system queries the with position of the character as well, so if A has look up table of '55,123,233,...' and B has look up table of '111,222,56,...', a spell with first letters AB would result with '55,222' and if first letters would be BA the result would be '111,123' instead.

These numbers go through math operation I am not sure has a name. Effectively it's binarry 'zipper' where they in their binary format take first bit of value A and B and put them in that order, then repeats to next bit of both values. so 1111 and 0000 would become 10101010, or 1010 and 1010 would become 11001100.

This 'binary zipper' is done to also random number generated at round start that is total of 64 bits but may be split into smaller numbers for compatability reasons. Each operation is done to next 8 bits and looped back once all 8 of 8 bit chunks are operated on.

Any 'splits' done by comma or other sentence ending character creates brand new 64 bit number to be operated on except bit shifted by 1 to right.
This should prevent spamming of same word to create too predictable chain.

Once all that math is done. the 64 bit numbers are passed on to the function responsible for determining what each chunk of bits mean.

### Chunks of bits
Chunks of the final 'magic number' may be any size determined by possible choises. If there are 4 delivery methods, the size of first chunk is 2 bits *(0-3)*. If the number of options does not fit in neat chunk, excess just uses modulo expression to loop back in the options available. 

When character casts the spell they recieve 'item' in their hand that they can use in harm mode to 'fire' it. When fired, spell stores the owner of the spell:
- `hCaster`: character responsible for casting the spell.
- `vOrigin`: postion of caster
- `vPosition`: position vector where effect could happen. Not normalized so that direct point target spells can work. This can be used for direction from `vOrigin` location when firing projectile.

First chunk of spell is delivery method. These are things like Radial effect of caster, self casting, projectile, and anything else you can think of. Delivery method always should populate three key variables:
- `vPoint`: final position vector where effect should happen. Projectile hit location would be this.
- `hTargets[]`: array of 'victims' of the spell
- `fRadius`: float to imply volume/radius of the effect

Second chunk is domain of the effect. Domains can be best described by discreet systems in space station 14. Such as spawning entities, spawning tiles, creating gasses, creating liquids, adding components. Each domain requires very different final implementation but the implementation should be able to recieve same populated variables from previous sections.

Remaining chunks are used by the different domain implementations to pick effect relevant to them. The chunk is passed fully along with index of 'cursor position'.

If there are 'splits' in the spell, The first 'chunk' of new split will be used by 'chain style' to determine origin of the next delivery method. Caster will remain same but other variables should work as previously explained.4

## Delivery
Delivery Methods define how the spell effect ends up selecting it's 'victims'.

### Self
Applies spell effect on or around self:
- `fRadius`, Always populated for Domain effect but collects `hTargets[]` only if 2nd bit from previous is 1. *(4bits)*
- `uDeliverySpan`: if 0 or 1, effects caster only. 2 effects in radius, 3 effects both, caster and in radius. *(2bits)*

### Projectile
Send out projectile, applies effect on hit or when distance is reached.
- `fRadius`: Radius for domain stuff. *(4bits)*
- `fSpeed`: projectile speed *(4bits)*
- `bHasAoE`: boolean to check if add only the hit entity to `hTargets[]` or also things in `fRadius` *(1bit)*
- `bBounces`: Will projectile bounce? collects to `hTargets[]` when bounces. *(1bit)*

### Point/Direct
Directly delivers the spell at target point/entity.
- `fRadius`: Radius for domain stuff. *(4bits)*
- `bHasAoE`: if false, adds only the closest enity into `hTargets[]` *(1bit)*

### Trap/Rune
Creates trap that triggers the spell when entity with a minimum speed moves on top of the trap's bounding box. has short delay before can activate.
- `fRadius`: Radius for domain stuff. *(4bits)*
- `bHasAoE`: if false, adds only the trigger enity into `hTargets[]` *(1bit)*
- `uSprite`: symbol from selection, instead of modulo operator, all values outside of the bit range make the rune invisible. *(6bits)*

### Edible
Creates random food item that triggers the effect at the victim much like self target.
- `uDeliverySpan`: if 0 or 1, effects activator only. 2 effects in radius, 3 effects both, activator and in radius. *(2bits)*
- `uFood`: one of possible disguises for the spell. Adds sprite and description. *(6bits)*
- `fRadius`: Always populated for Domain effect but collects `hTargets[]` only if 2nd bit from previous is 1. *(4bits)*

## Domains
Domains require different things from their implementations. Here are some ideas how some of the domains could be implemented.

### Gases
Effect takes `vPoint` and adds/replaces Gas based on `fRadius`. Some variables:
- `bReplace`: Instead of adding, replaces the gas. *(1bit)*
- `uDomainSpan`: if 0 or 1, add gas only to atmos. 2 add gas into containers (lungs, canisters, tanks). 3 is both. *(2bits)*
- `fTemp`: Temperature in Kelvin. *(6bits)*
- `fMoles`: How many moles to add. *(6bits)*
- `uGasId`: Id of the gas to add. *(3bits)*

### Reagents
Effect takes `vPoint` and adds/replaces 'Solusion' based on `fRadius`. Some variables:
- `bReplace`: Instead of adding, replaces the gas. *(1bit)*
- `uDomainSpan`: if 0 or 1, add reagents as spill. 2 add reagents into solutions  (stomack, bloodstream, beakers, food). 3 is both. *(2bits)*
- `fTemp`: Temperature in Kelvin. *(6bits)*
- `fUnits`: How many 'u' to add. *(6bits)*
- `uReagentId`: *(16bits)*

### Entities
Effect takes `vPoint` and spawns entities (prototypes) based on `fRadius`. Can spawn any item with `Physics` in their prototype. Some variables:
- `uEntityId`: Entity prototype to spawn *(32bits)*
- `uForce`: coefficient between `fRadius` and applied velocity from `vPoint` to simulate explosion of items. *(4bits)*

### Components
Effect takes `vPoint` and adds components to entities based in `fRadius`. Component list is curated with min and max default values.
- `uComponentId`: Component to add. I don't like curated list as solution but not sure if doing this with blanket adding any comp onent is possible, block list maybe? *(8-32bits)*

## Chain Style
When spell has multiple 'splits', all of the ones except first one may have different origin. Example: Projectile when hitting a target, sends aditional projectile directly back at the caster with the secondary effect.

### Override
Override things based on first target and cast next.
- `bCaster`: Boolean to override `hCaster`, if none in `hTargets[]`, fail. *(1bit)*
- `bOrigin`: Boolean to override `vOrigin`, if none in `hTargets[]`, fail. *(1bit)*
- `uPosition`: override `vPosition`. 0 = nope. 1 = target's facing. 2 = caster's facing, 3 = caster's location. *(2bits)*

### Splinter
Cas+