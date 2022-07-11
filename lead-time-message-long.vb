LET LeadWeeks = ROUND(DIVIDE(REGEX_MATCHES(VALUE("LeadDays"),"^\\d*"),7),0) IN

LET POdate = IF (VALUE("Date of Next Available PO"),                                #Check if NextAvailablePODate
                 
                IF(GT(VALUE("Date of Next Available PO"),TODAY()),                  #Check if date is in the future
                    FORMAT_DATETIME(VALUE("Date of Next Available PO"),'%Y-%m-%d', '%B %d'), #If yes, then display the future MM -YYYY
                    FORMAT_DATETIME(ADD(TODAY(),14),'%Y-%m-%d', '%B %d')) #If NO, then display the (current date + 14) in MM -YYYY
                ,
                
                 IF ( EQUAL(LeadWeeks,1),
                     CONCATENATE("in ",LeadWeeks," week"),
                     CONCATENATE("in ",LeadWeeks," weeks")
                
                    )
                
                ) IN
   
   

LET qty =
REGEX_MATCHES(
      VALUE("NetAvailByLoc"),
      "\\d*")
IN


LET totalinv =
ROUND(ARRAY_SUM(qty),0)
IN


LET sale = IF(LT(VALUE("NormalPrice"),VALUE("ProductPrice")), VALUE("NormalPrice"),VALUE("ProductPrice") ) IN 

LET IsonSale = IF(LT(sale,VALUE("MSRP")),"Yes","No") IN



IF (VALUE("ObsoleteStatusCode"), #Check obsoletestatus code exists to exclude messaging on the PARENT level in Salsify


IF 																												
( 
	  GT(totalinv,"0") 		#Level0 IF and Condition - Is in Stock?	
	  
	   
	   , 	                                 #Level0 True - IN STOCK				


	            IF		 		#Level1 IF
	            (
		            AND			#Level1 -Condition(Is D/T/OTB and IsOnSale is Yes and is Clearance?)
		            (
			            OR
			            (
				            EQUALS(VALUE("ObsoleteStatusCode"),"D"), 
				            EQUALS(VALUE("ObsoleteStatusCode"),"T"), 
				            EQUALS(VALUE("ObsoleteStatusCode"),"OTB"),
				            EQUALS(VALUE("ObsoleteStatusCode"),"D-OU"), 
				            EQUALS(VALUE("ObsoleteStatusCode"),"T-OU"), 
				            EQUALS(VALUE("ObsoleteStatusCode"),"OTBM")
				            
            		    	), 
            			  EQUALS(IsonSale,"Yes"),
			         EQUALS(VALUE("Lead Time Type"),"Clearance")
                    )
		            ,				#Level1 True
		
	            	"This is a clearance item, which means it is final sale and quantities are limited. It is ready for delivery within 2 days. At time of purchase, please select pickup or delivery."
	              ,					#Level1 False 
	      
                	IF		 		#Level2 IF
	              (
		              AND			#Level2 Condition(Is D/T/OTB and is FLOOR MODEL?)
		              (
			              OR
			              (
				              EQUALS(VALUE("ObsoleteStatusCode"),"D"), 
				              EQUALS(VALUE("ObsoleteStatusCode"),"T"), 
				              EQUALS(VALUE("ObsoleteStatusCode"),"OTB"),
				              EQUALS(VALUE("ObsoleteStatusCode"),"D-OU"), 
				              EQUALS(VALUE("ObsoleteStatusCode"),"T-OU"), 
				              EQUALS(VALUE("ObsoleteStatusCode"),"OTBM")
			              ), 
			              EQUALS(VALUE("Lead Time Type"),"Floor Model")
		                )				
                    ,				#Level2 True

		                  IF (   EQUALS(IsonSale,"Yes"),  #Level3 IF
				
				                  IF(    EQUALS(totalinv,1),      #Level3 True - Level5 IF

					                  "Only a floor model remains of this item which is reflected in the price. All floor models are thoroughly inspected before they are delivered and you will be advised of any imperfections. At time of purchase, please select pickup or delivery.",
				                    "Only floor models remain of this item which is reflected in the price. All floor models are final sale and available for delivery within 2 days.  They are thoroughly inspected before they are delivered and you will be advised of any imperfections. At time of purchase, please select pickup or delivery."
				                    )
	         		            , 		            #Level3 False
			
		                        		IF (EQUALS(IsonSale,"No"),
				                          IF(    EQUALS(totalinv,1),      

					                            "Only a floor model remains of this item. All floor models are thoroughly inspected before they are delivered and you will be advised of any imperfections. At time of purchase, please select pickup or delivery.",
				                              "Only floor models remain of this item. All floor models are final sale and available for delivery within 2 days. They are thoroughly inspected before they are delivered and you will be advised of any imperfections. At time of purchase, please select pickup or delivery."
				                            )	            
		                            )	   
		    

		                    ), #Level3 Closed   

				"This item is in stock and ready to be picked up, shipped or delivered within 2 days. At time of purchase, please select pickup or delivery."  #All other In stock options
		
		              )#Level2 Closed	

		        )#Level1 Closed
	 	
	
	
	
	
,				#-------------Level0 False - NOT IN STOCK
           IF (OR				#Level1 IF Condition - if D/T/OTB/OTBM
		       ( 
			EQUALS(VALUE("ObsoleteStatusCode"),"D"), 
			EQUALS(VALUE("ObsoleteStatusCode"),"T"), 
			EQUALS(VALUE("ObsoleteStatusCode"),"OTB"),
			EQUALS(VALUE("ObsoleteStatusCode"),"D-OU"), 
			EQUALS(VALUE("ObsoleteStatusCode"),"T-OU"), 
			EQUALS(VALUE("ObsoleteStatusCode"),"OTBM")
        		        )
	 	,				#Level1 - True

			IF			#Level2 IF Condition - Is on Sale is Yes and LTT is Clearance
			(AND 
				(
		  			EQUALS(IsonSale,"Yes"),
					EQUALS(VALUE("Lead Time Type"),"Clearance")
		
    				)
				,		#Level2 True

		                 “This item has been sold. Please inquire in our showroom for more information.”
		                ,		#Level2 False
   				 “This item has sold out and is no longer available."
			)

	       ,					#Level 1- False
    
	IF					#Level2 IF Condition - NS/FN?
	(
		OR	
		(	EQUALS(VALUE("ObsoleteStatusCode"),"NS"), 
			EQUALS(VALUE("ObsoleteStatusCode"),"FN") 
         	)
		,				#Level 2- True
      
			IF				#Level3 IF Condition - is LTT Special Order?
			(
				EQUALS(VALUE("Lead Time Type"),"Special Order")	
			,				#Level3 True
				CONCATENATE("This special order item is expected to be ready for delivery ”,POdate,". At time of purchase, please select pickup or delivery.") #Level 3 True
         		, 				#Level3 False

				IF				#Level4 I Condition - is LTT Made to Order?
				(	
					EQUALS(VALUE("Lead Time Type"),"Made to Order") # Level 4 Condition
					, 
						CONCATENATE("This item is made to order and is expected to be ready for delivery ",POdate,". At time of purchase, please select pickup or delivery.")	# Level 4 True
					,
						CONCATENATE("This item is currently out of stock and is expected to be ready for delivery ",PODate,". At time of purchase, please select pickup or delivery.")   # Level 4 False
				) 				#Level 4 IF closed
			) 				#Level 3 IF closed
	
	       ,					#Level 2 FALSE
	
	IF			#Level3 IF Condition - DBA , LTT is Clearance and on sale?
	( 
		AND				
		(	EQUALS(VALUE("ObsoleteStatusCode"),"DBA"),
			EQUALS(VALUE("Lead Time Type"),"Clearance"),
			EQUALS(IsonSale,"Yes")
		)
		,		#Level3 True
		
			CONCATENATE("This is a clearance item, which means it is final sale and quantities are limited. It is expected to be ready for delivery ",POdate,". At time of purchase, please select pickup or delivery.")
		,		#Level3 False


    IF (  AND	#Level4 DBA and not clearance	
			    (	EQUALS(VALUE("ObsoleteStatusCode"),"DBA"),
				    NOT(EQUALS(VALUE("Lead Time Type"),"Clearance”))
			    )	
			,     #Level4 True
                        CONCATENATE("This item is currently out of stock and is expected to be ready for delivery ",POdate,". At time of purchase, please select pickup or delivery.”)

      , 
		
		    IF 			#Level5 IF Condition - is AR, AR-W ?
		        (
			      OR ( 
			
			        EQUALS(VALUE("ObsoleteStatusCode"),"AR"),
			        EQUALS(VALUE("ObsoleteStatusCode"),"AR-W")
			     
			           )
			
			        
			            ,
               		CONCATENATE("This item is currently out of stock and is expected to be ready for delivery ",POdate,". At time of purchase, please select pickup or delivery.") #Level4 True
               		,
               		CONCATENATE("This item is currently out of stock. Please contact us for more information.") #Level4 False - default status for out of stock (AR-L) will display this
                )			#Level 5 closed
				   )   # Level4 closed    
	)			#Level3 closed

	) #L2 Close
 ) #L1 Close
) #L0 Close

) #checking obsoletestatus code